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
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // Injetamos o UserManager para conseguir criar/remover na tabela AspNetUsers
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/UsersApi
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MyUserDTO>>> GetUsers()
        {
            var users = await _context.MyUsers // Ajustado para MyUsers conforme o teu DbContext
                                      .Where(u => !u.IsDeleted)
                                      .Select(u => new MyUserDTO
                                      {
                                          Id = u.Id,
                                          Name = u.Name,
                                          CellPhone = u.CellPhone,
                                          Photo = u.Photo
                                      })
                                      .ToListAsync();
            return users;
        }

        // GET: api/UsersApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MyUserDTO>> GetUser(int id)
        {
            var user = await _context.MyUsers
                                     .Where(u => u.Id == id && !u.IsDeleted)
                                     .Select(u => new MyUserDTO
                                     {
                                         Id = u.Id,
                                         Name = u.Name,
                                         CellPhone = u.CellPhone,
                                         Photo = u.Photo
                                     })
                                     .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return user;
        }

        // PUT: api/UsersApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, MyUserDTO userDto)
        {
            if (id != userDto.Id) return BadRequest();

            var userInDb = await _context.MyUsers.FindAsync(id);
            if (userInDb == null || userInDb.IsDeleted) return NotFound();

            //Carregar o id do utilizador que tem sessão iniciada, que foi passado para o token
            var userLoggado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");


            //Validação que vê se é o utilizador que iniciou sessão ou se é admninistrador
            if (!isAdmin && userInDb.IdUser != userLoggado)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Acesso negado. Apenas o próprio utilizador ou um Administrador podem editar este perfil."
                });
            }

            
            userInDb.Name = userDto.Name;
            userInDb.CellPhone = userDto.CellPhone;
            

            _context.Entry(userInDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                else throw;
            }

            //Mensagem de sucesso
            return Ok(new
            {
                sucesso = true,
                mensagem = "Perfil editado com sucesso!"
            });
        }

        // POST: api/UsersApi
        [HttpPost]
        [AllowAnonymous] // O registo de novos utilizadores tem de ser público (sem exigir token JWT)
        public async Task<ActionResult<MyUserDTO>> PostUser(MyUserDTO userDto)
        {
            //Criar as credenciais de acesso na tabela AspNetUsers (Identity)
            var identityUser = new IdentityUser
            {
                UserName = userDto.email,
                Email = userDto.email,
                EmailConfirmed = true // Garante que a conta fica logo ativa para fazer login
            };

            //O UserManager valida as regras da password e faz o Hash de forma segura automaticamente
            var identityResult = await _userManager.CreateAsync(identityUser, userDto.password);

            if (!identityResult.Succeeded)
            {
                //Se a password for fraca ou o email já existir, devolvemos os erros nativos do Identity
                return BadRequest(identityResult.Errors);
            }

            //Criar o perfil com os 4 campos correspondentes na tua tabela MyUsers
            MyUser newUser = new()
            {
                Name = userDto.Name,
                CellPhone = userDto.CellPhone,
                Photo = "noImage.jpg",
                IsDeleted = false, // Conta nova entra sempre ativa
                IdUser = identityUser.Id // AQUI APONTAMOS PARA O GUID GERADO NO STEP 1!
            };

            try
            {
                _context.MyUsers.Add(newUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Mecanismo de Segurança (Rollback): Se a gravação na tua tabela MyUsers falhar por algum motivo,
                // eliminamos o utilizador que criámos no Step 1 para não deixar lixo desemparelhado na BD!
                await _userManager.DeleteAsync(identityUser);
                return BadRequest();
            }

            //Projetar o DTO de retorno limpo (omitindo a password por questões de segurança!)
            var returnDto = new MyUserDTO
            {
                Id = newUser.Id,
                Name = newUser.Name,
                CellPhone = newUser.CellPhone,
                email = identityUser.Email ?? "",
                Photo = newUser.Photo,
                IsDeleted = newUser.IsDeleted
            };

            return CreatedAtAction("GetUser", new { id = newUser.Id }, returnDto);
        }

        
        // DELETE: api/UsersApi/5 -> Eliminação de utilizador em ambas as tabelas AspNetUsers e MyUsers
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            //Carrega o Utilizador que tem sessão iniciada, registado no token e verfifica e guarda o role do proprio 
            var userLoggado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            //Procurar o perfil do utilizador na tua tabela MyUsers
            var user = await _context.MyUsers.FindAsync(id);
            if (user == null) return NotFound(new{mensagem = "O utilizador especificado não foi encontrado." });

            if (!isAdmin && user.IdUser != userLoggado)
            {
                // 403 Forbidden Tem sessão iniciada mas não revela as caracteristicas necessárias para eliminar
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = "Acesso negado. Apenas o dono do perfil ou um Administrador podem eliminar esta conta."
                });
            }

            //Remover todos os comentários em posts feito pelo mesmo
            var postComments = await _context.PostComments.Where(c => c.AutorFK == id).ToListAsync();
            foreach (var postComment in postComments)
            {
                postComment.AutorFK = null;
            }

            // Remover todos os likes efetuados pelo utilizador
            var likesDone = await _context.Likes.Where(l => l.LikeAutorFK == id).ToListAsync();
            _context.Likes.RemoveRange(likesDone);

            //Remover todas as avaliações que recebeu no perfil
            var receivedRatings = await _context.Comments.Where(r => r.RecipientFK == id).ToListAsync();
            _context.Comments.RemoveRange(receivedRatings);

            //Ir buscar todos os posts (anúncios) que o utilizador fez
            var postsDone = await _context.Posts.Where(p => p.OwnerFK == id).ToListAsync();
            var postsIds = postsDone.Select(p => p.Id).ToList();

            //Remover todos os likes dados nos posts deste utilizador
            var likesOnMyPosts = await _context.Likes.Where(l => postsIds.Contains(l.PostFK)).ToListAsync();
            _context.Likes.RemoveRange(likesOnMyPosts);

            //Remover todos os comentários feitos nos posts deste utilizador
            var commentsOnMyPosts = await _context.PostComments.Where(c => postsIds.Contains(c.PostFK)).ToListAsync();
            _context.PostComments.RemoveRange(commentsOnMyPosts);

            //Apagar as fotos dos anuncios do utilizador 
            foreach (var post in postsDone)
            {

                // Constrói o caminho completo até à pasta dos teus anúncios (ajusta o nome da pasta "imagens_anuncios")
                var caminhoFotoPost = Path.Combine(_webHostEnvironment.WebRootPath, "images", post.Photo);

                // Verifica se o ficheiro existe mesmo no disco antes de tentar apagar para não dar erro
                if (System.IO.File.Exists(caminhoFotoPost))
                {

                    //Apaga do disco
                    System.IO.File.Delete(caminhoFotoPost);
                }
            }


            //Apagar os posts do utilizador
            _context.Posts.RemoveRange(postsDone);


            //Manter as avaliações que este utilizador deixou em outros perfis, mas remover o Autor
            var sentComments = await _context.Comments.Where(s => s.AutorFK == id).ToListAsync();
            foreach (var comment in sentComments)
            {
                comment.AutorFK = null;
            }

            //Procura o registo correspondente na tabela AspNetUsers através do IdUser
            var identityUser = await _userManager.FindByIdAsync(user.IdUser);

            if (identityUser != null)
            {
                // Apaga o utilizador do Identity para ele perder imediatamente o acesso ao sistema/login
                await _userManager.DeleteAsync(identityUser);
            }

            //Lógica para eliminar a foto de perfil do disco
            if (!string.IsNullOrEmpty(user.Photo) && user.Photo != "noImage.jpg")
            {
                // Constrói o caminho completo até à pasta das fotos de perfil (ajusta o nome da pasta "imagens_perfis")
                var caminhoFotoPerfil = Path.Combine(_webHostEnvironment.WebRootPath, "images1", user.Photo);

                if (System.IO.File.Exists(caminhoFotoPerfil))
                {
                    //Apaga do disco 
                    System.IO.File.Delete(caminhoFotoPerfil);
                }
            }

            //Proceder a eliminação do utilizador 
            _context.MyUsers.Remove(user);

            
            await _context.SaveChangesAsync();
            return Ok(new
            {
                sucesso = true,
                mensagem = "O utilizador foi elimnado com sucesso e com isso todos os seus comentarios,likes,posts,etc foram removidos com sucesso "
            });
        }

        private bool UserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}