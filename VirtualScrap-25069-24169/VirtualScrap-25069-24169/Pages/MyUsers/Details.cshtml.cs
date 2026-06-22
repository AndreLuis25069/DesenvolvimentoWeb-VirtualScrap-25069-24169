using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;
        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                .Include(u => u.LikesList)            
                    .ThenInclude(l => l.LikedPost)    
                    .ThenInclude(p => p.PostCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
                

            if (MyUser == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int Id, int ratingId, string ratingText, int ratingStars)
        {
            //Se for um utilizador não autenticado é redirecionado para a página do login
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            //Verificação para quando alguém  tenta remover o required pela consola o comentário nao ser aceite em branco
            if (string.IsNullOrWhiteSpace(ratingText))
            {
                return RedirectToPage(new { id = Id });
            }

            //Ir buscar o comentario/avaliação á base de dados
            var rating = await _context.Comments.FindAsync(ratingId);
            if (rating == null) return NotFound();

            //Ir buscar o id do utilizador que tem sessão iniciada
            var loggedUserId = _userManager.GetUserId(User);
            var userInMyUsers = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == loggedUserId);

            if (userInMyUsers == null) return Challenge();


            // Verificar se é dono ou admin 
            if (!User.IsInRole("Admin") && rating.AutorFK != userInMyUsers.Id)
            {
                return RedirectToPage(new { id = Id });
            }

            rating.Description = ratingText;
            rating.Rating = ratingStars;
            _context.Attach(rating).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Id });
        }


        //Função para eliminar comentarios dada a escolha do utilizador Admin ou Dono do próprio comentário 
        public async Task<IActionResult> OnPostDeleteCommentAsync(int? Id, int ratingId) {
            //Verificar se o utilizador está com sessão iniciada, caso contrário é redirecionado para a página de Login
            if (!User.Identity.IsAuthenticated)
                return Challenge();

            //Ir buscar o comentario/avaliação á base de dados
            var rating = await _context.Comments.FindAsync(ratingId);
            if (rating == null) return NotFound();

            //Ir buscar o id do utilizador que tem sessão iniciada
            var loggedUserId = _userManager.GetUserId(User);
            var userInMyUsers = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == loggedUserId);

            //Se o utilizador não existir na base de dados é redirecionado para a página de login.
            if (userInMyUsers == null) return Challenge();

            //Se o utilizador não tiver o role de Administrador ou for o autor do mesmo comentário a página é recarregada e a edição não tem valor
            //Isto serve para reforçar a segurança em caso de contornos pela consola
            if(!User.IsInRole("Admin") && rating.AutorFK != userInMyUsers.Id)
            {
                return RedirectToPage(new { id = Id });
            }


            //Proceder á eliminação do comentário
            _context.Comments.Remove(rating);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = Id });
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