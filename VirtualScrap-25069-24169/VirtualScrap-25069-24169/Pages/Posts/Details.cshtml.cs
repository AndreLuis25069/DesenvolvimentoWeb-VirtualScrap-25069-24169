using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class DetailsModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Post Post { get; set; } = default!;

        
        //Propriedade para capturar o texto do formulário
        [BindProperty]
        [Required(ErrorMessage = "O comentário não pode estar vazio.")]
        [StringLength(500)]
        public string NovoComentarioTexto { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.PostCategory)
                .Include(p => p.Commentaries)
                    .ThenInclude(c => c.Autor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post is not null)
            {
                Post = post;
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                // Se falhar, recarrega o post com todas as relações para a página não cair
                Post = await _context.Posts
                    .Include(p => p.PostCategory)
                    .Include(p => p.Commentaries)
                        .ThenInclude(c => c.Autor)
                    .FirstOrDefaultAsync(m => m.Id == id);
                return Page();
            }

            // 1. Ir buscar o ID do Identity
            var identityUserId = _userManager.GetUserId(User);

            // 2. Encontrar o MyUser personalizado baseado no GUID do Identity
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUserId);

            if (myUser == null)
            {
                ModelState.AddModelError(string.Empty, "Erro ao identificar o utilizador no sistema.");

                // Recarrega os dados da página antes de devolver a View com o erro
                Post = await _context.Posts
                    .Include(p => p.PostCategory)
                    .Include(p => p.Commentaries)
                        .ThenInclude(c => c.Autor)
                    .FirstOrDefaultAsync(m => m.Id == id);
                return Page();
            }

            // 3. Instanciar o comentário de forma correta
            var comentarioParaGravar = new PostComment
            {
                Description = NovoComentarioTexto,
                CommentDate = DateTime.Now,
                PostFK = id,
                AutorFK = myUser.Id
            };

            // 4. Guardar na Base de Dados
            _context.PostComments.Add(comentarioParaGravar);
            await _context.SaveChangesAsync();

            // Faz redirect para o GET da própria página para limpar o input do formulário
            return RedirectToPage(new { id = id });
        }
    }
}