using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    //Garantir que é exigida uma autenticação

    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LikesController(ApplicationDbContext context)
        {
            _context = context;
        }


        //Configurar rota POST para criar um like
        //POST: api/Likes
        [HttpPost]
        //Apenas pessoas com sessão iniciada podem (ter token) podem inserir um like
        [Authorize]
        
        public async Task<IActionResult> PostLike([FromBody] LikeDTO likeDto)
        {
            //Carregar o utilizador que está no token
            var userLoggado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var autorMyUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == userLoggado);

            if (autorMyUser == null) {
                return NotFound(new { Sucesso = false, mensagem = "O utilizador não foi encontrado" });
            }
            

            //Partir para a verificação se o post existe na base de dados
            var post = await _context.Posts.FindAsync(likeDto.PostId);
            if (post == null)
            {
                return NotFound(new { sucesso = false, mensagem = "Este anuncio não existe na base de dados" });
            }


            //Verificar se esse like ja existe
            var likeExiste = await _context.Likes.AnyAsync(l => l.LikeAutorFK == autorMyUser.Id && l.PostFK == likeDto.PostId);
            if (likeExiste)
            {
                return BadRequest(new { sucesso = false, mensagem = "O like que estás a tentar inserir já existe na base de dados" });
            }



            var novoLike = new Like
            {
                LikeAutorFK = autorMyUser.Id,
                PostFK = likeDto.PostId
            };

            //Guardar o like na base de dados
            _context.Likes.Add(novoLike);

            await _context.SaveChangesAsync();

            //Avisar que correu tudo bem com a inserção 
            return Ok(new { sucesso = true, message = "O like foi adicionado com sucesso" });
        }

        

        //Configuarar rota GET para listar os likes todos em JSON
        //GET: api/likes
        [HttpGet]
        [AllowAnonymous]
        //Permitir o get em caso de ser um utilizador não logado
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetLikes()
        {
            return await _context.Likes
                .Include(l => l.LikeAutor)
                .Include(l => l.LikedPost)
                .Select(l => new LikeDTO
                {
                    AutorId = l.LikeAutorFK,
                    PostId= l.PostFK,
                    AutorName = l.LikeAutor != null ? l.LikeAutor.Name : "Desconhecido",
                    PostTitle = l.LikedPost != null ? l.LikedPost.Title : "Artigo Removido"
                })
                .ToListAsync();
        }

        //Fazer o GET de um like por determinados IDs do Post que o like pertence e Autor
        //GET: api/Likes/5/6
        [HttpGet("{autorId}/{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<LikeDTO>> GetLike(int autorId, int postId)
        {
            var like = await _context.Likes
                //É feita a filtragem pelos 2 IDs
                 .Where(l => l.LikeAutorFK == autorId && l.PostFK == postId) // Filtra pelos dois IDs!
                 .Select(l => new LikeDTO
                 {
                     AutorId = l.LikeAutorFK,
                     PostId = l.PostFK,
                     AutorName = l.LikeAutor != null ? l.LikeAutor.Name : "Desconhecido",
                     PostTitle = l.LikedPost != null ? l.LikedPost.Title : "Artigo Removido"
                 })
                 .FirstOrDefaultAsync();

            if (like == null)
            {
                // Erro 404
                return NotFound("O like especificado não foi encontrado."); 
            }

            return like; 
        }


        //DELETE: api/Likes/3/15
        //Delete de um dado Like escolhido
        [HttpDelete("{autorId}/{postId}")]
        [Authorize]
        public async Task<IActionResult> DeleteLike(int autorId, int postId)
        {
            //Procurar o like usando os dois IDs
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.LikeAutorFK == autorId && l.PostFK == postId);

            if (like == null)
            {
                return NotFound("O like que tentou apagar não existe.");
            }

            //Carregar o id do utilizador que tem sessão iniciada, e foi enviado para dentro do token
            var userLoggado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            //Ligação com a tabela myusers, para ver o utilizador correspondente ao que tem sessão iniciada
            var autorDoLike = await _context.MyUsers
            .Where(u => u.Id == like.LikeAutorFK)
            .Select(u => u.IdUser)
            .FirstOrDefaultAsync();

            if (!isAdmin && autorDoLike != userLoggado)
            {
               //Mensagem de acesso negado, em caso de utilizador não for Admninistrador ou dono do like
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas o utilizador que deu o gosto ou um utilizador Administrador o podem remover."
                });
            }


            //Apagar o registo da base de dados
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                sucesso = true,
                mensagem = "Like removido com sucesso!"
            });

           
        }
    }
}
