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
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
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
                        
            if (post == null)
            {
                return NotFound();
            }

            var userIdLogado = _userManager.GetUserId(User);

            //Verifica se o utilizador logado é o dono do post ou se o dono do post existe ou se quem está a tentar aceder está logado
            if (!User.Identity.IsAuthenticated || post.PostOwner == null || post.PostOwner.IdUser != userIdLogado)
            {
                return RedirectToPage("./Index");
            }
            Post = post;
            ViewData["CategoryFK"] = new SelectList(_context.Categories, "Id", "Id");


            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Procura o post original para validar o verdadeiro dono antes de guardar as alterações
            var postOriginal = await _context.Posts
                .Include(p => p.PostOwner)
                .AsNoTracking() // Para não dar conflito no Entity Framework ao atualizar
                .FirstOrDefaultAsync(p => p.Id == Post.Id);

            var userIdLogado = _userManager.GetUserId(User);

            if (postOriginal == null || postOriginal.PostOwner == null || postOriginal.PostOwner.IdUser != userIdLogado)
            {
                return RedirectToPage("./Index");
            }

            _context.Attach(Post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(Post.Id))
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

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
