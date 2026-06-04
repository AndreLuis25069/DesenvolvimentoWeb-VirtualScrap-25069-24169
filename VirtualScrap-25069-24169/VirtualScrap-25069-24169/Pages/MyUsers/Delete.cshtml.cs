using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class DeleteModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DeleteModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
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
               .Include(u => u.PostsList)
               .FirstOrDefaultAsync(m => m.Id == id);
             
            //Se for nulo retorna erro
            if(MyUser == null)
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



            //Vai á base de dados buscar um utilizador com o mesmo id
            var myuser = await _context.MyUsers.FindAsync(id);
            //Se o utilizador não for nulo vai proceder para a elimação do mesmo
            if (myuser != null)
            {
                 
                MyUser = myuser;
                _context.MyUsers.Remove(MyUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
