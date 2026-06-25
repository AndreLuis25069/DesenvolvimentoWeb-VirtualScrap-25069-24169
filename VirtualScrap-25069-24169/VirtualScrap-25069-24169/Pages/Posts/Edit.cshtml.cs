using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;  
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;
        [BindProperty]
        public IFormFile? PostImage { get; set; }
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
            if (!User.Identity.IsAuthenticated ||  (post.PostOwner?.IdUser != userIdLogado && !User.IsInRole("Admin")))
            {
                return RedirectToPage("./Index");
            }
            Post = post;

            var price = Post.Price.ToString("F2");
            Post.AuxPrice = price;
            
            ViewData["CategoryFK"] = new SelectList(_context.Categories, "Id", "Name");


            return Page();
        }



        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            // Procura o post original para validar o verdadeiro dono antes de guardar as alterações
            var postOriginal = await _context.Posts
                .Include(p => p.PostOwner)
                .AsNoTracking() // Para não dar conflito no Entity Framework ao atualizar
                .FirstOrDefaultAsync(p => p.Id == Post.Id);

            

            if (!ModelState.IsValid)
            {
                ViewData["CategoryFK"] = new SelectList(_context.Categories, "Id", "Name", Post.CategoryFK);
                return Page();
            }

            

            //Ir buscar ao sistema o Id do utilizador que tem sessão iniciada
            var userIdLogado = _userManager.GetUserId(User);

            //Confirma se quem chegou até aqui é o dono do anuncio ou admnistrador, caso não seja o programa não aceita as alterações
            if (postOriginal == null ||  (postOriginal.PostOwner.IdUser != userIdLogado && !User.IsInRole("Admin")))
            {
                return RedirectToPage("./Index");
            }
            //Guardar o Guid da foto antiga caso o utilizador so deseje alterar o nome por exemplo, isto mantém a foto
            string fotoAntiga = postOriginal.Photo;

            bool novaImagemEnviada = false;

            if (PostImage == null)
            { 
                Post.Photo = fotoAntiga; 
            }else{
                if (PostImage.ContentType != "image/jpeg" && PostImage.ContentType != "image/png")
                {
                    //Reportar o erro e retornar a pagina
                    ModelState.AddModelError("ImagePost", "O ficheiro deve ser JPEG ou JPG ou PNG");
                    return Page();
                }

                //Processo de criar o GUID da fotografia nova
                var guidImagem = Guid.NewGuid().ToString() + Path.GetExtension(PostImage.FileName).ToLowerInvariant();
                Post.Photo = guidImagem;

                novaImagemEnviada = true;
            }

            //Bloco para converter a string do price auxiliar para o price em decimal, guardando o na base de dados
            if (!string.IsNullOrEmpty(Post.AuxPrice))
            {
                // Substitui pontos por vírgulas (ou vice-versa) para garantir que o sistema operativo 
                // do servidor não se baralha com as regras decimais da língua portuguesa
                string precoFormatado = Post.AuxPrice.Replace('.', ',');

                if (decimal.TryParse(precoFormatado, out decimal precoConvertido))
                {
                    Post.Price = precoConvertido; 
                }
            }
            else
            {
                // Se por acaso vier vazio, mantém o preço que já estava guardado originalmente
                Post.Price = postOriginal.Price;
            }


            Post.OwnerFK = postOriginal.OwnerFK;
            _context.Attach(Post).State = EntityState.Modified;

            try
            {   
                //Processo de guardar a imagem no disco 
                await _context.SaveChangesAsync();

                if (novaImagemEnviada)
                {
                    string imagePath = _webHostEnvironment.WebRootPath;
                    imagePath = Path.Combine(imagePath, "images");
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    //Criar o caminho da diretoria junto do GUID
                    string fullPath = Path.Combine(imagePath, Post.Photo);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await PostImage.CopyToAsync(stream);
                    }
                    
                }

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
