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
    public class DetailsModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public MyUser MyUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myuser = await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == id);

            if (myuser is not null)
            {
                MyUser = myuser;

                return Page();
            }

            return NotFound();
        }
    }
}
