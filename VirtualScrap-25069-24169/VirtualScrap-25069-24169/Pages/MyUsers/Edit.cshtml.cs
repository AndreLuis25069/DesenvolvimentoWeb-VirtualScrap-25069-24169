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
        public IFormFile? ProfileImage { get; set; }
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

            //Confirma se o utilizador logado é administrador ou o dono do perfil
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

            //Valida se o utilizador tentou entrar pelo link direto e não é administrador nem dono do perfil, se for o caso volta para a pagina inicial
            var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Vai buscar os  dados do utilizador real que está guardado na base de dados para este perfil
            var dadosReaisBD = await _context.MyUsers
                .Where(u => u.Id == MyUser.Id)
                .Select(u => new { u.IdUser, u.Photo })
                .FirstOrDefaultAsync();

            //Confirma se existem dados ou se o utilizador logado é administrador ou o dono do perfil, caso contrário volta para a pagina inicial
            if (dadosReaisBD == null || (!isAdmin && dadosReaisBD.IdUser != userIdLogado))
            {
                return RedirectToPage("/Index"); 
            }


            //Verificar se no campo de imagem no browser foi feito o upload de uma nova imagem
            if (ProfileImage != null)
            {
                // Verificar a extensão/tipo do ficheiro para segurança
                if (ProfileImage.ContentType != "image/jpeg" && ProfileImage.ContentType != "image/png" && ProfileImage.ContentType != "image/jpg")
                {
                    ModelState.AddModelError("ProfileImage", "O ficheiro deve ser do tipo JPEG, JPG ou PNG.");
                    return Page();
                }

                // Gera o novo nome único para a imagem nova
                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();
                MyUser.Photo = imageName;
            }
            else
            {
                //  Se não fez upload de imagem nova, mantém a imagem que ele já tinha guardada BD
                MyUser.Photo = dadosReaisBD.Photo;
            }

            // Forçamos a limpeza da validação do IdUser porque ele não vem do formulário e daria erro no ModelState
            ModelState.Remove("MyUser.IdUser");

          

            
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
                //Tenta guardar as alterações na base de dados
                await _context.SaveChangesAsync();
                //Se o utilizador fez upload de uma nova imagem, guarda a imagem no servidor
                if (ProfileImage != null && !string.IsNullOrEmpty(MyUser.Photo))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images1");
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    string fullPath = Path.Combine(imagePath, MyUser.Photo);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }
                }
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

            return RedirectToPage("./Details", new {id = MyUser.Id});
        }

        private bool MyUserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}
