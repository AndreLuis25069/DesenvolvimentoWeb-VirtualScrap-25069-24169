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
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        // Injetamos o UserManager para conseguir criar/remover na tabela AspNetUsers
        private readonly UserManager<IdentityUser> _userManager;

        public UsersApiController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            userInDb.Name = userDto.Name;
            userInDb.CellPhone = userDto.CellPhone;
            userInDb.Photo = userDto.Photo;

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

            return NoContent();
        }

        // POST: api/UsersApi
        [HttpPost]
        [AllowAnonymous] // O registo de novos utilizadores tem de ser público (sem exigir token JWT)
        public async Task<ActionResult<MyUserDTO>> PostUser(MyUserDTO userDto)
        {
            // STEP 1: Criar as credenciais de acesso na tabela AspNetUsers (Identity)
            var identityUser = new IdentityUser
            {
                UserName = userDto.email,
                Email = userDto.email,
                EmailConfirmed = true // Garante que a conta fica logo ativa para fazer login
            };

            // O UserManager valida as regras da password e faz o Hash de forma segura automaticamente
            var identityResult = await _userManager.CreateAsync(identityUser, userDto.password);

            if (!identityResult.Succeeded)
            {
                // Se a password for fraca ou o email já existir, devolvemos os erros nativos do Identity
                return BadRequest(identityResult.Errors);
            }

            // STEP 2: Criar o perfil com os 4 campos correspondentes na tua tabela MyUsers
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

            // STEP 3: Projetar o DTO de retorno limpo (omitindo a password por questões de segurança!)
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

        // =======================================================================
        // DELETE: api/UsersApi/5 -> ELIMINAÇÃO/BLOQUEIO NAS DUAS TABELAS
        // =======================================================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {

            // 1. Procura o perfil do utilizador na tua tabela do projeto
            var user = await _context.MyUsers.FindAsync(id);
            if (user == null) return NotFound();

            // 2. Procura o registo correspondente na tabela AspNetUsers através do IdUser
            var identityUser = await _userManager.FindByIdAsync(user.IdUser);

            if (identityUser != null)
            {
                // Apaga o utilizador do Identity para ele perder imediatamente o acesso ao sistema/login
                await _userManager.DeleteAsync(identityUser);
            }

            // 3. Faz o Soft Delete na tua tabela para manter o histórico de dados (anúncios, likes, etc.)
            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}