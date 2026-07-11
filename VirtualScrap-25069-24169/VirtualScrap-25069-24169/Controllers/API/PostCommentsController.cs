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

        //Endpoint para fazer get aos posts inseridos na base de dados
        // GET: api/PostComments ou api/PostComments?postId=12
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostComments([FromQuery] int? postId)
        {
            var query = _context.PostComments
                .Include(pc => pc.Autor)
                .Include(pc => pc.CommentedPost)
                .AsQueryable();

            //Se for passado um ID de anuncio alvo, só os desse são retornados.
            if (postId.HasValue)
            {
                query = query.Where(pc => pc.PostFK == postId.Value);
            }

            //Caso contrario (parâmetro vazio) mostram-se todos os comentarios existentes
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

        //Endpoint para ir buscar um comentário específico (commentId) dentro de um post específico (postId)
        // GET: api/PostComments/12/5  (Onde 12 é o Post e 5 é o Comentário)
        [AllowAnonymous]
        [HttpGet("{postId}/{commentId}")]
        public async Task<IActionResult> GetSpecificComment(int postId, int commentId)
        {
            var postComment = await _context.PostComments
                .Include(pc => pc.Autor)
                .Include(pc => pc.CommentedPost)
                //Os 2 IDs têm de bater certo
                .FirstOrDefaultAsync(pc => pc.PostFK == postId && pc.Id == commentId);

            if (postComment == null)
            {
                return NotFound("O comentário não existe ou não pertence a este anúncio.");
            }

            var dto = new PostCommentDTO
            {
                Id = postComment.Id,
                Description = postComment.Description,
                CommentDate = postComment.CommentDate,
                AutorFK = postComment.AutorFK,
                PostFK = postComment.PostFK,
                AutorName = postComment.Autor != null ? postComment.Autor.Name : "Utilizador Anónimo",
                CommentedPostTitle = postComment.CommentedPost != null ? postComment.CommentedPost.Title : "Anúncio Removido"
            };

            return Ok(dto);
        }



        //Endpoint para inserir um comentário num post
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
            if (myUser == null) return BadRequest("O Perfil do utilizador não registado na tabela MyUsers.");

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

        //Endpoint para eliminar um comentário de um post
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

            if (!isAdmin && !isOwner)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas o autor do comentário ou um utilizador Administrador o podem eliminar."
                });
            }

            _context.PostComments.Remove(postComment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comentário do anúncio removido com sucesso!" });
        }

        //Endpoint para editar um comentario de um post
        // PUT: api/PostComments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePostComment(int id, [FromBody] PostCommentDTO dto)
        {
            // Validar coerência de IDs
            if (id != dto.Id)
                return BadRequest("O ID enviado no URL não coincide com o ID do corpo do pedido.");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Procurar o comentário do anúncio
            var postComment = await _context.PostComments.FindAsync(id);
            if (postComment == null) return NotFound("Comentário do anúncio não encontrado.");

            // Identificar o autor através do bearer Token JWT
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = postComment.AutorFK == myUser?.Id;

            if (!isAdmin && !isOwner)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas o autor do comentário ou um utilizador Administrador o podem editar."
                });  
            }

            // Atualizar a descrição do comentário
            postComment.Description = dto.Description;

            _context.Entry(postComment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comentário do anúncio atualizado com sucesso!", comment = dto });
        }
    }
}