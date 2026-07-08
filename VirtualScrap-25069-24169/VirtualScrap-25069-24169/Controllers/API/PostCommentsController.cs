using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;
using VirtualScrap_25069_24169.Models.ViewModels;

namespace VirtualScrap_25069_24169.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PostCommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostCommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/PostComments ou api/PostComments?postId=12
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetPostComments([FromQuery] int? postId)
        {
            var query = _context.PostComments
                .Include(pc => pc.Autor)
                .Include(pc => pc.CommentedPost)
                .AsQueryable();

            if (postId.HasValue)
            {
                query = query.Where(pc => pc.PostFK == postId.Value);
            }

            var comments = await query
                .OrderByDescending(pc => pc.CommentDate)
                .Select(pc => new PostCommentDTO
                {
                    Id = pc.Id,
                    Description = pc.Description,
                    CommentDate = pc.CommentDate,
                    AutorFK = pc.AutorFK,
                    PostFK = pc.PostFK,
                    AutorName = pc.Autor != null ? pc.Autor.Name : "Utilizador Anónimo",
                    CommentedPostTitle = pc.CommentedPost != null ? pc.CommentedPost.Title : "Anúncio Removido"
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/PostComments
        [HttpPost]
        public async Task<IActionResult> CreatePostComment([FromBody] PostCommentDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Resolver ID do MyUser através do Token JWT
            var identityEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(identityEmail)) return Unauthorized();

            var identityUser = await _userManager.FindByEmailAsync(identityEmail);
            if (identityUser == null) return Unauthorized();

            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser.Id);
            if (myUser == null) return BadRequest("Perfil de utilizador não registado na tabela MyUsers.");

            // Validar se o Post de facto existe na base de dados
            var postExists = await _context.Posts.AnyAsync(p => p.Id == dto.PostFK);
            if (!postExists) return NotFound("O anúncio associado a este comentário não existe.");

            var postComment = new PostComment
            {
                Description = dto.Description,
                CommentDate = DateTime.Now,
                AutorFK = myUser.Id, // Forçado pelo Token por segurança
                PostFK = dto.PostFK
            };

            _context.PostComments.Add(postComment);
            await _context.SaveChangesAsync();

            dto.Id = postComment.Id;
            dto.AutorFK = myUser.Id;
            dto.AutorName = myUser.Name;

            return CreatedAtAction(nameof(GetPostComments), new { postId = postComment.PostFK }, dto);
        }

        // DELETE: api/PostComments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePostComment(int id)
        {
            var postComment = await _context.PostComments.FindAsync(id);
            if (postComment == null) return NotFound("Comentário do anúncio não encontrado.");

            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u!.IdUser == identityUser!.Id);

            // Regra: Só o Admin ou o dono do comentário do post o podem apagar
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = postComment.AutorFK == myUser?.Id;

            if (!isAdmin && !isOwner) return Forbid();

            _context.PostComments.Remove(postComment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comentário do anúncio removido com sucesso!" });
        }

        // PUT: api/PostComments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePostComment(int id, [FromBody] PostCommentDTO dto)
        {
            // 1. Validar coerência de IDs
            if (id != dto.Id)
                return BadRequest("O ID enviado no URL não coincide com o ID do corpo do pedido.");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Procurar o comentário do anúncio
            var postComment = await _context.PostComments.FindAsync(id);
            if (postComment == null) return NotFound("Comentário do anúncio não encontrado.");

            // 3. SEGURANÇA CRUCIAL: Identificar o autor através do Token JWT
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = postComment.AutorFK == myUser?.Id;

            if (!isAdmin && !isOwner)
            {
                return Forbid(); // 403 Forbidden se for outro utilizador qualquer
            }

            // 5. Atualizar a descrição do comentário
            postComment.Description = dto.Description;

            _context.Entry(postComment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comentário do anúncio atualizado com sucesso!", comment = dto });
        }
    }
}