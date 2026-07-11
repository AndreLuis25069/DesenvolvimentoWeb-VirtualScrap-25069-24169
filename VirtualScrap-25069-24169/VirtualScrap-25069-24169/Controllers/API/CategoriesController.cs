using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;
using VirtualScrap_25069_24169.Data.Model.ViewModels;

namespace VirtualScrap_25069_24169.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Endpoint que vai buscar todas as categorias
        // GET: api/Categories
        // Aberto ao público para que todos possam filtrar anúncios por categoria
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        //Endpoint para ir buscar a categoria pelo ID 
        // GET: api/Categories/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Categoria não encontrada.");

            var dto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(dto);
        }

        //Endpoint para criar uma nova categoria 
        // POST: api/Categories
        // So é possivel criar categorias se o utilizador que está no token de autenticação for adminstrador 
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategorySimplerDTO dto)
        {
            //Verificação se o utilizador é admin, apenas dentro do metodo para conseguir enviar uma mensagem de aviso quando é um utilizador comum a tentar dar POST.
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas os utilizadores Administradores podem criar novas categorias!"
                });
            }
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //Não deixa criar categorias com nomes duplicados
            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            if (exists) return BadRequest($"A categoria '{dto.Name}' ja existe no sistema!");

            var category = new Category
            {
                Name = dto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Mapeia para o DTO de leitura completa para dar resposta ao cliente
            var resultDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, resultDto);
        }

        //Endpoint para atualizar o nome de uma dada categoria.
        // PUT: api/Categories/5
        //Só o Administrador pode mudar o nome das categorias
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategorySimplerDTO dto)
        {
            //Verificação se o utilizador é admin, apenas dentro do metodo para conseguir enviar uma mensagem de aviso quando é um utilizador comum a tentar dar PUT.
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas os utilizadores Administradores podem editar  categorias!"
                });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Categoria não encontrada.");

            // Validar se o novo nome já não está a ser usado por outra categoria
            var exists = await _context.Categories
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower());

            if (exists) return BadRequest("Já existe outra categoria com esse nome.");

            // Atualiza o nome
            category.Name = dto.Name;

            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Categoria atualizada com sucesso!", category = new CategoryDTO { Id = category.Id, Name = category.Name } });
        }

        //EndPoint para eliminar uma categoria
        // DELETE: api/Categories/5
        // Só um Administrador pode apagar categorias
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            //Verificação se o utilizador é admin, apenas dentro do metodo para conseguir enviar uma mensagem de aviso quando é um utilizador comum a tentar dar DELETE.
            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    sucesso = false,
                    mensagem = "Acesso negado. Apenas os utilizadores Administradores podem eliminar categorias."
                });
            }
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound("Categoria não encontrada.");

            // Regra de Integridade: Verificar se existem posts associados a esta categoria antes de apagar
            var hasPosts = await _context.Posts.AnyAsync(p => p.CategoryFK == id);
            if (hasPosts)
            {
                return BadRequest("Não podes eliminar esta categoria! Existem anúncios ativos associados a ela. Altera primeiro os anúncios ou elimina-os.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Categoria eliminada com sucesso!" });
        }
    }
}