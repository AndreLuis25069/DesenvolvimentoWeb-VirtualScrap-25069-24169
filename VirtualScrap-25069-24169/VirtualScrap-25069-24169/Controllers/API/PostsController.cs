using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;
using VirtualScrap_25069_24169.Models.ViewModels;

namespace VirtualScrap_25069_24169.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Posts
        // Nota: Deixei [AllowAnonymous] aqui para que qualquer pessoa (mesmo deslogada) possa ver os anúncios do site. 
        // Se quiseres que só utilizadores logados vejam, basta apagar a linha [AllowAnonymous].
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.PostOwner)
                .Include(p => p.PostCategory)
                .OrderByDescending(p => p.PostDate)
                .Select(p => new PostDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CellPhone = p.CellPhone,
                    PostDate = p.PostDate,
                    Photo = p.Photo,
                    Price = p.Price,
                    Localizacao = p.Localizacao,
                    // Assumindo que a tua classe Category tem a propriedade .Name (ou .Designacao)
                    CategoryName = p.PostCategory != null ? p.PostCategory.Name : "Sem Categoria",
                    OwnerName = p.PostOwner != null ? p.PostOwner.Name : "Utilizador Anónimo"
                })
                .ToListAsync();

            return Ok(posts);
        }

        // GET: api/Posts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.PostOwner)
                .Include(p => p.PostCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound("Anúncio não encontrado.");

            var dto = new PostDTO
            {
                Id = post.Id,
                Title = post.Title,
                Description = post.Description,
                CellPhone = post.CellPhone,
                PostDate = post.PostDate,
                Photo = post.Photo,
                Price = post.Price,
                Localizacao = post.Localizacao,
                CategoryName = post.PostCategory != null ? post.PostCategory.Name : "Sem Categoria",
                OwnerName = post.PostOwner != null ? post.PostOwner.Name : "Utilizador Anónimo"
            };

            return Ok(dto);
        }

        // POST: api/Posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Descobrir o MyUser logado através do Token JWT
            var identityEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(identityEmail)) return Unauthorized();

            var identityUser = await _userManager.FindByEmailAsync(identityEmail);
            if (identityUser == null) return Unauthorized();

            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser.Id);
            if (myUser == null) return BadRequest("Perfil do anunciante não encontrado no sistema.");

            // Buscar a Categoria correta pelo nome enviado pelo DTO
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);
            if (category == null) return BadRequest($"A categoria '{dto.CategoryName}' não existe no sistema.");

            var post = new Post
            {
                Title = dto.Title,
                Description = dto.Description,
                CellPhone = dto.CellPhone,
                PostDate = DateTime.Now, // Data gerada no servidor
                Photo = string.IsNullOrEmpty(dto.Photo) ? "default_post.jpg" : dto.Photo,
                Price = dto.Price,
                Localizacao = dto.Localizacao,
                CategoryFK = category.Id,
                OwnerFK = myUser.Id // Vinculado de forma segura ao dono do Token
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Atualizar o DTO de retorno
            dto.Id = post.Id;
            dto.OwnerName = myUser.Name;
            dto.PostDate = post.PostDate;

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, dto);
        }

        // PUT: api/Posts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] PostDTO dto)
        {
            if (id != dto.Id) return BadRequest("O ID enviado no URL não coincide com o do corpo do pedido.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound("Anúncio não encontrado.");

            // Validar se quem edita é o Dono ou Admin
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = post.OwnerFK == myUser?.Id;

            if (!isAdmin && !isOwner) return Forbid(); // 403 Se não for o dono ou admin

            // Validar se a nova categoria enviada existe
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == dto.CategoryName);
            if (category == null) return BadRequest($"A categoria '{dto.CategoryName}' não existe.");

            // Atualizar os campos permitidos
            post.Title = dto.Title;
            post.Description = dto.Description;
            post.CellPhone = dto.CellPhone;
            post.Price = dto.Price;
            post.Localizacao = dto.Localizacao;
            post.CategoryFK = category.Id;

            if (!string.IsNullOrEmpty(dto.Photo))
                post.Photo = dto.Photo;

            _context.Entry(post).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Anúncio atualizado com sucesso!", post = dto });
        }

        // DELETE: api/Posts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            // Carrega o Post incluindo os Comentários e os Likes para aplicar a regra Cascade via código
            var post = await _context.Posts
                .Include(p => p.Commentaries)
                .Include(p => p.LikesList)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound("Anúncio não encontrado.");

            // Validar se quem elimina é o Dono ou Admin
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = post.OwnerFK == myUser?.Id;

            if (!isAdmin && !isOwner) return Forbid();

            // 💥 REGRA CASCADE MANUAL (Garante a limpeza total dos filhos na Base de Dados)
            if (post.Commentaries != null && post.Commentaries.Any())
            {
                _context.PostComments.RemoveRange(post.Commentaries);
            }

            if (post.LikesList != null && post.LikesList.Any())
            {
                // Remove todos os likes associados a este post
                _context.Likes.RemoveRange(post.LikesList);
            }

            // Eliminar o post principal
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Anúncio e todos os seus comentários/likes foram eliminados com sucesso!" });
        }
    }
}