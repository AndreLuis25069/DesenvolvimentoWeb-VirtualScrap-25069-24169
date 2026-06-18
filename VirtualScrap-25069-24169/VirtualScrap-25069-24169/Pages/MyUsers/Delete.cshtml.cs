using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class DeleteModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        public DeleteModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<MyUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public MyUser MyUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Carrega o Utilizador da base de dados 
            MyUser = await _context.MyUsers
                //Incluir a lista de posts que esse utilizador tem
                .Include(p => p.PostsList)
                .FirstOrDefaultAsync(m => m.Id == id);
                   
               
             
            //Se for nulo retorna erro
            if(MyUser == null)
            {
                return NotFound();
            }

            MyUser.PostsList = await _context.Posts
                .Where(p => p.OwnerFK == MyUser.Id)
                .ToListAsync();


            return Page();
           
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }



            //Vai á base de dados buscar um utilizador com o mesmo id
            var myuser = await _context.MyUsers.FindAsync(id);
                
            //Se o utilizador não for nulo vai proceder para a elimação do mesmo
            if (myuser != null)
            {

                //Remover todos os likes efetuados pelo utilizador
                var likesDone = await _context.Likes.Where(l => l.LikeAutorFK == id).ToListAsync();
                _context.Likes.RemoveRange(likesDone);

                //Remover todas as avaliaçoes Recebidas pelo utilizador
                var receivedRatings = await _context.Comments.Where(r => r.RecipientFK == id).ToListAsync();
                foreach(var r in receivedRatings) { r.AutorFK = null;}
                
                //
                var postsDone = await _context.Posts.Where(p => p.OwnerFK == id).ToListAsync();
                _context.Posts.RemoveRange(postsDone);
                await _context.SaveChangesAsync();

                MyUser = myuser;
                await _userManager.DeleteAsync(MyUser);
                

              
            }

            return RedirectToPage("./Index");
        }
    }
}
