using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class DetailsModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public MyUser MyUser { get; set; } = default!;

        [BindProperty]
        public Comment Comment { get; set; } = default!;

        
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            MyUser = await _context.MyUsers
                .Include(u => u.ReceivedComments)
                    .ThenInclude(e => e.Autor)
                .FirstOrDefaultAsync(m => m.Id == id);
                

            if (MyUser == null)
            {
                return NotFound();
            }

            return Page();
        } 

        
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MyUser = await _context.MyUsers
           .Include(u => u.ReceivedComments)
               .ThenInclude(e => e.Autor)
           .FirstOrDefaultAsync(m => m.Id == id);

            if (MyUser == null)
            {
                return NotFound();
            }




            var userGUID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userGUID))
            {
                ModelState.AddModelError(string.Empty, "Precisas de ter a sessão iniciada.");
                return Page();
            }

            var myUserCommentator = await _context.MyUsers
                .FirstOrDefaultAsync(u => u.IdUser == userGUID);

            if(myUserCommentator == null)
            {
                ModelState.AddModelError(string.Empty, "Não existe este utilizador");
                return Page();
            }


            int myUserId = myUserCommentator.Id;

            

            if (!ModelState.IsValid)
            {
                return Page();
            }
           
            try
            {
                
                Comment.CommentDate = DateTime.Now;
                Comment.RecipientFK = id.Value;
                Comment.AutorFK = myUserId;

                _context.Comments.Add(Comment);
                await _context.SaveChangesAsync();

                // Recarrega o perfil atualizado para o utilizador ver o seu comentário na lista
                MyUser = await _context.MyUsers
                    .Include(u => u.ReceivedComments)
                        .ThenInclude(e => e.Autor)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível adicionar o Comentário.");
            }

            return Page();
        }
    }
}