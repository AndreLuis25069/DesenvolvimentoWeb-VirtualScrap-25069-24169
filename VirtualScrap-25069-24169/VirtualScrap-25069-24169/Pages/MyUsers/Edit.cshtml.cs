using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [BindProperty]
        public MyUser MyUser { get; set; } = default!;
        public IFormFile ProfileImage { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            //Se o id for nulo retorna erro 
            if (id == null)
            {
                return NotFound();
            }

            //Carregar os dados do utilizador que tem o id igual ao correspondente, vindos da base de dados
            var myuser =  await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == id);

            //Se o objeto da base de dados for nulo retorna erro
            if (myuser == null)
            {
                return NotFound();
            }
            MyUser = myuser;

            // --- VALIDAÇÃO DE SEGURANÇA NO GET ---
            var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && MyUser.IdUser != userIdLogado)
            {
                return RedirectToPage("/Index");
            }

            HttpContext.Session.SetInt32("MyUserId", MyUser.Id);
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()

        {
            //Guardar o id do MyUser que foi guardado nos cookies
            var idUserCookie = HttpContext.Session.GetInt32("MyUserId");

            //Se o cookie estiver vazio volta para tras
            if(idUserCookie == null)
            {
                ModelState.AddModelError(string.Empty, "Os cookies expiraram ou houve adulteração dos dados");
                return Page();
            }

            //Se o objeto que vem do formulario tiver o id diferente do do cookie volta para tras e não altera nada
            if (idUserCookie != MyUser.Id)
            {
                return RedirectToPage("/Index");
            }

            // --- VALIDAÇÃO DE SEGURANÇA NO POST ---
            var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Vai buscar o IdUser real que está guardado na base de dados para este perfil
            var idUserRealNaBD = await _context.MyUsers
                .Where(u => u.Id == MyUser.Id)
                .Select(u => u.IdUser)
                .FirstOrDefaultAsync();

            if (!isAdmin && idUserRealNaBD != userIdLogado)
            {
                return RedirectToPage("/Index"); // Bloqueio total se os IDs não baterem certo
            }


           
            //Verificar se não é PNG, JPEG, JPG
            if (ProfileImage.ContentType != "image/jpeg" && ProfileImage.ContentType != "image/png")
            {
                //Reportar o erro e retornar a pagina
                ModelState.AddModelError("ImagePost", "O ficheiro deve ser JPEG ou JPG ou PNG");
                return Page();
            }

            //Variavel para guardar um GUID do ficheiro
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
            //Atribuir ao valor da variavel photo, do modelo da BD
            MyUser.Photo = imageName;

            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //Anexar o modelo que veio do formulario
            _context.Attach(MyUser).State = EntityState.Modified;
            //Alertar a entity framework para não alter o IdUser no sql
            _context.Entry(MyUser).Property(x => x.IdUser).IsModified = false;
            try
            {
                await _context.SaveChangesAsync();

                //Processo de guardar a imagem no disco rigido do servidor
                string imagePath = _webHostEnvironment.WebRootPath;
                imagePath = Path.Combine(imagePath, "images1");
                if (!Directory.Exists(imagePath))
                {
                    Directory.CreateDirectory(imagePath);
                }

                //Combina o caminho da diretoria criada com o GUID da imagem
                string fullPath = Path.Combine(imagePath, MyUser.Photo);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                return Page();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MyUserExists(MyUser.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool MyUserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}
