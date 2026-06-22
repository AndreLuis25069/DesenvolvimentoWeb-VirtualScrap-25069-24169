using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<MyUser> MyUser { get;set; } = default!;

        public async Task OnGetAsync()
        {
            MyUser = await _context.MyUsers.ToListAsync();
        }

        public async Task<IActionResult> OnPostTornarAdminAsync(string idUser)
        {
            if (string.IsNullOrEmpty(idUser)) return NotFound();
            var identityUser = await _userManager.FindByIdAsync(idUser);
            if (identityUser != null) await _userManager.AddToRoleAsync(identityUser, "Admin");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRetirarAdminAsync(string idUser)
        {
            if (string.IsNullOrEmpty(idUser)) return NotFound();
            var identityUser = await _userManager.FindByIdAsync(idUser);
            if (identityUser != null && _userManager.GetUserId(User) != idUser)
            {
                await _userManager.RemoveFromRoleAsync(identityUser, "Admin");
            }
            return RedirectToPage();
        }
    }
}
