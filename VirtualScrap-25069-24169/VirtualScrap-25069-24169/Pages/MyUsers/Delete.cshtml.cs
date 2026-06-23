using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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
            //Incluir a lista de posts que esse utilizador tem
            MyUser = await _context.MyUsers
            .Include(p => p.PostsList)
            .FirstOrDefaultAsync(m => m.Id == id);



            //Se for nulo retorna erro
            if (MyUser == null)
            {
                return NotFound();
            }

            MyUser.PostsList = await _context.Posts
                .Where(p => p.OwnerFK == MyUser.Id)
                .ToListAsync();

            //Verifica se o utilizador que tem sessão iniciada é administrador ou o dono do perfil
            var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && MyUser.IdUser != userIdLogado)
            {
                return RedirectToPage("/Index");
            }

            return Page();

        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            //Verifica se o utilizador que tem sessão iniciada é administrador ou o dono do perfil
            var userIdLogado = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Vai buscar o IdUser real que está guardado na base de dados para este perfil
            var idUserRealNaBD = await _context.MyUsers
                .Where(u => u.Id == MyUser.Id)
                .Select(u => u.IdUser)
                .FirstOrDefaultAsync();

            if (!isAdmin && idUserRealNaBD != userIdLogado)
            {
                return RedirectToPage("/Index"); // Bloqueio total se os IDs não baterem certo
            }


            //Vai á base de dados buscar um utilizador com o mesmo id
            var myuser = await _context.MyUsers.FindAsync(id);

            //Se o utilizador não for nulo vai proceder para a elimação do mesmo
            if (myuser != null)
            {
                bool deletingLoggedUser = (myuser.IdUser == User.FindFirstValue(ClaimTypes.NameIdentifier));

                //Remover comentarios feitos em posts 
                var postComments = await _context.PostComments.Where(c => c.AutorFK == id).ToListAsync();
                foreach (var postComment in postComments)
                {
                    postComment.AutorFK = null;
                }


                //Remover todos os likes efetuados pelo utilizador
                var likesDone = await _context.Likes.Where(l => l.LikeAutorFK == id).ToListAsync();
                _context.Likes.RemoveRange(likesDone);

                //Remover todas as avaliaçoes Recebidas pelo utilizador
                var receivedRatings = await _context.Comments.Where(r => r.RecipientFK == id).ToListAsync();
                _context.Comments.RemoveRange(receivedRatings);

                //Remover todos os posts que o utilizador fez
                var postsDone = await _context.Posts.Where(p => p.OwnerFK == id).ToListAsync();
                _context.Posts.RemoveRange(postsDone);
                await _context.SaveChangesAsync();


                //Manter os comentarios feitos mas remover a chave forasteira para que não exista problemas no momento de eliminar os utilizadores
                var sentComments = await _context.Comments.Where(s => s.AutorFK == id).ToListAsync();
                foreach (var comment in sentComments)
                {
                    //Aqui atribuimos o valor nulo ao campo da chave do autor
                    comment.AutorFK = null;
                }

                //Guardar o id deste user na tabela AspNetUsers, vindo da chave estrangeira IdUser em MyUser para que possamos eliminar a conta  do mesmo
                var gUID = myuser.IdUser;

                //Se o gUID não for nulo, vamos procurar a conta na tabela AspNetUsers correspondente e removê-la
                if (!string.IsNullOrEmpty(gUID))
                {
                    var identityAccount = await _context.Users.FirstOrDefaultAsync(u => u.Id == gUID);
                    if (identityAccount != null)
                    {
                        _context.Users.Remove(identityAccount);
                    }
                }

                MyUser = myuser;
                //Remover o utilizador da base de dados em Myuser e guardar as alterações
                _context.MyUsers.Remove(MyUser);
                await _context.SaveChangesAsync();

                //Verificar se o utilizador que está a ser eliminado é o mesmo que está logado, se for, terminar sessão 
                //Redirecionar para a página inicial, caso contrário, permanecer na página de utilizadores

                if (deletingLoggedUser)
                {
                    await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return RedirectToPage("/Index");
                }
            }

            return RedirectToPage("./Index");
        }
    }
}

