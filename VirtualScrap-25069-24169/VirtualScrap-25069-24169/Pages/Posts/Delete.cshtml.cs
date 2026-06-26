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

namespace VirtualScrap_25069_24169.Pages.Posts
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                        .Include(p => p.PostOwner)
                        .FirstOrDefaultAsync(m => m.Id == id);

            var userIdLogado = _userManager.GetUserId(User);

            //Verifica se o utilizador logado é o dono do post ou se o dono do post existe ou se quem está a tentar aceder está logado
            if (!User.Identity.IsAuthenticated || (post.PostOwner?.IdUser != userIdLogado && !User.IsInRole("Admin")))
            {
                return RedirectToPage("./Index");
            }

            if (post is not null)
            {
                Post = post;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                            .Include(p => p.PostOwner)
                            .FirstOrDefaultAsync(m => m.Id == id);

            // Se o post já não existir, redireciona ou dá NotFound
            if (post == null)
            {
                return NotFound();
            }

            //Vai buscar o ID do utilizador que está logado a tentar apagar
            var userIdLogado = _userManager.GetUserId(User);

            //Se não for o dono real, redireciona para fora e não apaga nada!
            if (!User.Identity.IsAuthenticated || (post.PostOwner?.IdUser != userIdLogado && !User.IsInRole("Admin")))
            {
                return RedirectToPage("./Index");
            }

            //Eliminar todos os likes que esse post tem 
            var likesOfPost = await _context.Likes
                .Where(l => l.PostFK == post.Id)
                .ToListAsync();
            _context.Likes.RemoveRange(likesOfPost);

            //Eliminar todos os comentários que esse post tem
            var postComments = await _context.PostComments
                .Where(c => c.PostFK == post.Id)
                .ToListAsync();
            _context.PostComments.RemoveRange(postComments);
            //Se passou na validação, então sim, remove-se da base de dados
            Post = post;
            _context.Posts.Remove(Post);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
