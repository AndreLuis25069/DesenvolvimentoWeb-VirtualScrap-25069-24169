using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;

        private readonly  IWebHostEnvironment _webHostEnvironment;
        public CreateModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            
            _context = context;
            _webHostEnvironment = webHostEnvironment;
         
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryFK"] = new SelectList(_context.Categories.OrderBy(d=>d.Name), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        [BindProperty]
        [Required(ErrorMessage = "O ficheiro de imagem é obrigatório.")]
        public IFormFile ImagePost { get; set; } 

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {




           

            //Verificar se o utilizador deu input no campo
            if (ImagePost == null)
            {
                //Reportar o erro e retornar a pagina
                ModelState.AddModelError("ImagePost", "A imagem é obrigatoria");
                ViewData["CategoryFK"] = new SelectList(_context.Categories.OrderBy(d => d.Name), "Id", "Name");
                return Page();
            }
            //Verificar se não é PNG, JPEG, JPG
            if (ImagePost.ContentType != "image/jpeg" && ImagePost.ContentType != "image/png")
            {
                //Reportar o erro e retornar a pagina
                ModelState.AddModelError("ImagePost", "O ficheiro deve ser JPEG ou JPG ou PNG");
                return Page();
            }

            //Variavel para guardar um GUID do ficheiro
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(ImagePost.FileName).ToLowerInvariant();
            //Atribuir ao valor da variavel photo, do modelo da BD
            Post.Photo = imageName;


            //Processo de Conversão do valor (string) inserido pelo utilizador no campo de input(preço) para um valor decimal diretamente para a BD
            Post.Price = Convert.ToDecimal(Post.AuxPrice.Replace('.', ','),
                                               new CultureInfo("pt-PT"));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentMyUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser.ToString() == userId);
            Post.PostOwner = currentMyUser;
            Post.CellPhone = Post.PostOwner.CellPhone;

            if (!ModelState.IsValid)
            {
                ViewData["CategoryFK"] = new SelectList(_context.Categories.OrderBy(d => d.Name), "Id", "Name");
                return Page();
            }

          
            
            try
            {
                _context.Posts.Add(Post);
                await _context.SaveChangesAsync();

                //Processo de guardar a imagem no disco rigido do servidor
                string imagePath = _webHostEnvironment.WebRootPath;
                imagePath = Path.Combine(imagePath, "images");
                if (!Directory.Exists(imagePath)){
                    Directory.CreateDirectory(imagePath);
                }


                //Combina o caminho da diretoria criada com o GUID da imagem
                string fullPath = Path.Combine(imagePath, Post.Photo);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ImagePost.CopyToAsync(stream);
                }
                return RedirectToPage("./Index");

            }
            catch(Exception)
            {
                ModelState.AddModelError(string.Empty, "Nao foi possivel adicionar o Post");
                return Page();
            }
            }
    }
}
