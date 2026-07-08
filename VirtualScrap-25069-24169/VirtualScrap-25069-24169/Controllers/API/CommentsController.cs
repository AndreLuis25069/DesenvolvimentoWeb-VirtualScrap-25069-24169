using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    // Força o uso do Token JWT Bearer para todo o controlador
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Comments ou api/Comments?recipientId=3
        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] int? recipientId)
        {
            var query = _context.Comments
                .Include(c => c.Autor)
                .Include(c => c.Recipient)
                .AsQueryable();

            // Se passarem o ID do destinatário na query string, filtra automaticamente
            if (recipientId.HasValue)
            {
                query = query.Where(c => c.RecipientFK == recipientId.Value);
            }

            var comments = await query
                .OrderByDescending(c => c.CommentDate)
                .Select(c => new CommentDTO
                {
                    Id = c.Id,
                    Description = c.Description,
                    CommentDate = c.CommentDate,
                    AutorFK = c.AutorFK,
                    RecipientFK = c.RecipientFK,
                    Rating = c.Rating,
                    AutorName = c.Autor != null ? c.Autor.Name : "Utilizador Anónimo",
                    RecipientName = c.Recipient != null ? c.Recipient.Name : "Desconhecido"
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Buscar o email do utilizador atual a partir das Claims do Token JWT
            var identityEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(identityEmail)) return Unauthorized();

            var identityUser = await _userManager.FindByEmailAsync(identityEmail);
            if (identityUser == null) return Unauthorized();

            // Encontrar o registo correspondente na tabela MyUsers para obter o ID numérico (int)
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser.Id);
            if (myUser == null) return BadRequest("Perfil de utilizador não encontrado no sistema.");

            //Impede que um user faça um comentário no seu proprio perfil
            if (myUser.Id == dto.RecipientFK)
            {
                return BadRequest("Não podes deixar uma avaliação ou comentário no teu próprio perfil, rei!");
            }

            // Validar se o destinatário da avaliação de facto existe
            var recipientExists = await _context.MyUsers.AnyAsync(u => u.Id == dto.RecipientFK);
            if (!recipientExists) return NotFound("O utilizador destinatário da avaliação não existe.");

            // Mapear o DTO para o Modelo da Base de Dados
            var comment = new Comment
            {
                Description = dto.Description,
                CommentDate = DateTime.Now,
                AutorFK = myUser.Id, // Injetado automaticamente por segurança via Token
                RecipientFK = dto.RecipientFK,
                Rating = dto.Rating
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Atualizar o DTO com as informações geradas para responder ao cliente
            dto.Id = comment.Id;
            dto.AutorFK = myUser.Id;
            dto.AutorName = myUser.Name;

            return CreatedAtAction(nameof(GetComments), new { recipientId = comment.RecipientFK }, dto);
        }

        // PUT: api/Comments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentDTO dto)
        {
            if (id != dto.Id)
                return BadRequest("O ID enviado no URL não coincide com o ID do corpo do pedido.");

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound("Avaliação não encontrada.");

            // Descobrir quem está a tentar fazer o pedido através do Token JWT
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            // REGRA DE SEGURANÇA: Apenas o Admin OU o próprio Autor podem atualizar
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = comment.AutorFK == myUser?.Id;

            if (!isAdmin && !isOwner)
            {
                return Forbid(); // Retorna 403 Forbidden se for outro utilizador
            }

            // Atualizar os campos permitidos
            comment.Description = dto.Description;
            comment.Rating = dto.Rating;

            _context.Entry(comment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Avaliação atualizada com sucesso!", comment = dto });
        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound("Avaliação não encontrada.");

            // Descobrir quem está a tentar fazer o pedido através do Token JWT
            var identityEmail = User.Identity?.Name;
            var identityUser = await _userManager.FindByEmailAsync(identityEmail ?? "");
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUser!.Id);

            // REGRA DE SEGURANÇA: Apenas o Admin OU o próprio Autor podem apagar
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = comment.AutorFK == myUser?.Id;

            if (!isAdmin && !isOwner)
            {
                return Forbid(); // Retorna 403 Forbidden
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Avaliação eliminada com sucesso!" });
        }
    }
}