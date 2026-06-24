using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
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

        public int MyUserIdLogado { get; set; }
        public int totalLikes { get; set; }

        //Propriedade para capturar o texto do formulário
        [BindProperty]
        [Required(ErrorMessage = "O comentário não pode estar vazio.")]
        [StringLength(500)]
        public string NovoComentarioTexto { get; set; } = "";

        // Propriedade para controlar o estado do botão no Front-end
        public bool JaTemLike { get; set; }

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

            if (post is null)
            {
                
                return NotFound();
            }

            Post = post;

            //Guardar a contagem de likes que esse Post tem.
            totalLikes = await _context.Likes.CountAsync(l => l.PostFK == post.Id);

            // Verificar se o utilizador logado já deu like neste post
            if (User.Identity.IsAuthenticated)
            {
                var identityUserId = _userManager.GetUserId(User);
                var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUserId);

                if (myUser != null)
                {
                    // Vê se existe algum registo na tabela Like com o ID do user e do Post
                    JaTemLike = await _context.Set<Like>()
                        .AnyAsync(l => l.LikeAutorFK == myUser.Id && l.PostFK == post.Id);

                    MyUserIdLogado = myUser.Id;
                }
            } 

            return Page();
        }

        // HANDLER PARA EDITAR O COMENTÁRIO
        public async Task<IActionResult> OnPostEditComentarioAsync(int id, int comentarioId, string TextoEditado)
        {

            
            //Se o utilizador não estiver com sessão iniciada irá ser redirecionado para a página de login
            if (!User.Identity.IsAuthenticated) return Challenge();
            
            //Garante que o comentário não pode ser vazio, caso seja  removido o required pela linha de consola de forma a aumentar a segurança
            if (string.IsNullOrWhiteSpace(TextoEditado))
            {
                return RedirectToPage(new { id = id });
            }

            //Vai buscar o comentario com o dado id á base de dados 
            var comentario = await _context.PostComments.FindAsync(comentarioId);
            if (comentario == null) return NotFound();

            //Guardar o Id do utilizador que está com a sessão iniciada 
            var identityUserId = _userManager.GetUserId(User);
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUserId);
            if (myUser == null) return Challenge();

            // Segurança: Dono ou Admin
            if (!User.IsInRole("Admin") && comentario.AutorFK != myUser.Id)
            {
                return RedirectToPage(new { id = id });
            }

            // Atualiza o texto e a BD
            comentario.Description = TextoEditado;
            _context.Attach(comentario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = id });
        }

        // HANDLER PARA ELIMINAR UM COMENTÁRIO
        public async Task<IActionResult> OnPostDeleteComentarioAsync(int id, int comentarioId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            // Procurar o comentário na BD
            var comentario = await _context.PostComments.FindAsync(comentarioId);
            if (comentario == null)
            {
                return NotFound();
            }

            // Ir buscar o utilizador logado para validar a segurança
            var identityUserId = _userManager.GetUserId(User);
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUserId);

            if (myUser == null)
            {
                return Challenge();
            }

            var isAdmin = User.IsInRole("Admin");

            // SEGURANÇA TOTAL: Só apaga se for Admin OU se for o verdadeiro dono do comentário
            if (!isAdmin && comentario.AutorFK != myUser.Id)
            {
                return RedirectToPage(new { id = id }); // Se for alguém a tentar injetar IDs, dá refresh sem fazer nada
            }

            // Remover e guardar
            _context.PostComments.Remove(comentario);
            await _context.SaveChangesAsync();

            // Faz refresh à própria página de detalhes
            return RedirectToPage(new { id = id });
        }

        // HANDLER PARA ADICIONAR / REMOVER LIKE
        public async Task<IActionResult> OnPostLikeAsync(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge();
            }

            var identityUserId = _userManager.GetUserId(User);
            var myUser = await _context.MyUsers.FirstOrDefaultAsync(u => u.IdUser == identityUserId);

            if (myUser == null)
            {
                return Challenge();
            }

            // Procura se o Like já existe
            var likeExistente = await _context.Set<Like>()
                .FirstOrDefaultAsync(l => l.LikeAutorFK == myUser.Id && l.PostFK == id);

            if (likeExistente != null)
            {
                // Se já existe, o utilizador clicou para retirar o Like (Remove)
                _context.Set<Like>().Remove(likeExistente);
            }
            else
            {
                // Se não existe, cria um novo registo (Add)
                var novoLike = new Like
                {
                    LikeAutorFK = myUser.Id,
                    PostFK = id
                };
                _context.Set<Like>().Add(novoLike);
            }

            await _context.SaveChangesAsync();

            // Faz refresh à página de detalhes do post
            return RedirectToPage(new { id = id });
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