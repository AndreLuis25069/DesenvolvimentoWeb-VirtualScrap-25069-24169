using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public MyUser MyUser { get; set; } = default!;
        public IFormFile ProfileImage { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myuser =  await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (myuser == null)
            {
                return NotFound();
            }
            MyUser = myuser;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            //Verificar se o utilizador deu input no campo
            if (ProfileImage == null)
            {
                //Reportar o erro e retornar a pagina
                ModelState.AddModelError("ProfileImage", "A imagem é obrigatoria");
                
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

            _context.Attach(MyUser).State = EntityState.Modified;

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
