# Contexto do Projeto E-Sports (ASP.NET Core)

> Ficheiro auto-gerado para partilha de contexto de código.

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VirtualScrapDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Program.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\ApplicationDbContext.cs`

```csharp
﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data.Model; 

namespace VirtualScrap_25069_24169.Data
{
    
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MyUser> MyUsers { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; } 
        public DbSet<Category> Categories { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        
            base.OnModelCreating(builder);

            /// <summary>
            /// Metodo para fazer com que no uso de DateTimeNow no projeto, a data seja gerada
            /// no momento que os dados são guardados na base de dados, 
            /// e não com a data certa em que o utilizador faz a operação no programa.
            /// </summary>
            builder.Entity<Comment>()
                .Property(c => c.CommentDate)
                .HasDefaultValueSql("GETDATE()");

            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um utilizador, os comentarios do mesmo efetuados noutros perfis ou 
            /// anuncios não sejam eliminados em CASCADE.
            /// </summary>
            builder.Entity<Comment>()
            .HasOne(c => c.Autor)
            .WithMany(u => u.SentComments) 
            .HasForeignKey(c => c.AutorFK)
            .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um utilizador, os comentarios recebidos pelo mesmo sejam eliminados
            /// em CASCADE.
            /// </summary>
            builder.Entity<Comment>()
            .HasOne(c => c.Recipient)
            .WithMany(u => u.ReceivedComments) 
            .HasForeignKey(c => c.RecipientFK)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\Model\Category.cs`

```csharp
﻿using System.ComponentModel.DataAnnotations;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome da categoria
        /// </summary>
        public string Name { get; set; } = "";

    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\Model\Comment.cs`

```csharp
﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class Comment
    {
        /// <summary>
        /// Chave primária para cada comentário, será chave estrangeira no perfil do utilizador
        /// </summary>
        [Key]
        public int Id { get; set; }

        ///<summary>
        /// Conteudo do comentário/Avaliação
        /// </summary>
        [Display(Name ="Descrição")]
        [Required(ErrorMessage ="A {0} é de escrita obrigatória")]
        [StringLength(500)]
        public string Description { get; set; } = null!;

        ///<summary>
        ///Data de quando foi feito o comentário
        /// </summary>
        [DataType(DataType.Date)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CommentDate { get; set; } = DateTime.Now;

        ///<summary>
        ///Autor do Comentário/Avaliação deixada num perfil
        /// </summary>
        [ForeignKey(nameof(Autor))]
        public int AutorFK { get; set; }
        [ValidateNever]

        ///<summary>
        ///Objeto do tipo MyUser para o autor
        ///</summary>
        public MyUser Autor { get; set; } = null!;

        ///<summary>
        ///Autor do Comentário/Avaliação deixada num perfil
        /// </summary>
        [ForeignKey(nameof(Recipient))]
        public int RecipientFK { get; set; }
        [ValidateNever]

        ///<summary>
        ///Objeto do tipo MyUser para o destinatário
        ///</summary>
        public MyUser Recipient { get; set; } = null!;
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\Model\Like.cs`

```csharp
﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{   

    /// <summary>
    /// Chave Primaria Composta, para cada like, pois um like é uma ponte entre um utilizador e um anuncio 
    /// </summary>
    [PrimaryKey(nameof(LikeAutorFK), nameof(PostFK))]
    public class Like
    {   
       
        ///<summary>
        ///Autor do Like deixado num anuncio
        /// </summary>
        [ForeignKey(nameof(LikeAutor))]
        public int LikeAutorFK { get; set; }



        ///<summary>
        ///Objeto do tipo MyUser para o autor do like
        ///</summary>
         [ValidateNever]
        public MyUser LikeAutor { get; set; } = null!;


        /// <summary>
        /// Referência ao post a que pertence o like
        /// </summary>
        [ForeignKey(nameof(LikedPost))]
        public int PostFK { get; set; }


        ///<summary>
        ///Objeto do tipo Post/Anuncio
        ///</summary>
        [ValidateNever]
        public Post LikedPost { get; set; } = null!;

    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\Model\MyUser.cs`

```csharp
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class MyUser
    {
        [Key] //Primary Key
        public int Id { get; set; }
        ///<summary>
        ///Nome do Utilizador.
        ///</summary>

        [Display(Name="Nome completo do Utilizador")]
        [Required(ErrorMessage ="O {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Name { get; set; } = "";

        ///<summary>
        ///Numero de telefone do Utilizador
        /// </summary>
        [Display(Name="Numero de Telefone do Utilizador")]
        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}",ErrorMessage ="O {0} é de preenchimento obrigatório")]
        public string CellPhone { get; set; }


        ///<summary>
        ///Lista de comentários feita ao utilizador
        /// </summary>
        [InverseProperty(nameof(Comment.Autor))] 
        public ICollection<Comment> SentComments { get; set; } = [];


        ///<summary>
        ///Lista de comentários feita Pelo utilizador
        /// </summary>
        [InverseProperty(nameof(Comment.Recipient))]
        public ICollection<Comment> ReceivedComments { get; set; } = [];



        ///<summary>
        ///Chave Forasteira para ligar com a tabela de autenticação
        /// </summary>

        public string IdUser { get; set; } = null!;


        ///<summary>
        ///Lista de anuncios feitos pelo utilizador
        /// </summary>
        public ICollection<Post> PostsList { get; set; } = [];
        


        ///<summary>
        ///Lista de likes que este utilizador realizou
        ///</summary>
        public ICollection<Like> LikesList { get; set; } = [];


        ///<summary>
        ///Estado que diz se o utilizador foi "removido" ou não
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Data\Model\Post.cs`

```csharp
﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualScrap_25069_24169.Data.Model
{
    public class Post
    {
        [Key] //Primary Key
        public int Id { get; set; }

        ///<summary>
        ///Título do post (ou nome do produto a ser vendido)
        ///</summary>

        [Display(Name = "Título")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Title { get; set; } = "";


        ///<summary>
        ///Descrição do post
        ///</summary>

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório!")]
        [StringLength(150)]
        public string Description { get; set; } = "";

        ///<summary>
        ///Lista de likes referentes ao post
        /// </summary>

        public ICollection<Like> LikesList { get; set; } = [];

        ///<summary>
        ///Contacto do vendedor
        /// </summary>

        [StringLength(19)]
        [RegularExpression(@"\+?[0-9]{9,18}", ErrorMessage = "Ocorreu um erro!")]
        public string CellPhone { get; set; } = "";

        ///<summary>
        ///Data em que o post foi publicado
        /// </summary>

        [DataType(DataType.Date)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime PostDate { get; set; } = DateTime.Now;

        ///<summary>
        ///Foto do post
        /// </summary>

        [Display(Name = "Foto")]
        [Required(ErrorMessage = "A {0} é de preenchimento obrigatório!")]
        [StringLength(50)]
        public string Photo { get; set; } = "";

        ///<summary>
        ///Categoria do post
        /// </summary>

        
        [ValidateNever]
        public Category PostCategory { get; set; } = null!;

        /// <summary>
        /// Chave estrangeira para referenciar a categoria
        /// </summary>

        [Display(Name = "Categoria")]
        [ForeignKey(nameof(PostCategory))]
        public int CategoryFK { get; set; }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Error.cshtml`

```html
﻿@page
@model ErrorModel
@{
    ViewData["Title"] = "Error";
}

<h1 class="text-danger">Error.</h1>
<h2 class="text-danger">An error occurred while processing your request.</h2>

@if (Model.ShowRequestId)
{
    <p>
        <strong>Request ID:</strong> <code>@Model.RequestId</code>
    </p>
}

<h3>Development Mode</h3>
<p>
    Swapping to the <strong>Development</strong> environment displays detailed information about the error that occurred.
</p>
<p>
    <strong>The Development environment shouldn't be enabled for deployed applications.</strong>
    It can result in displaying sensitive information from exceptions to end users.
    For local debugging, enable the <strong>Development</strong> environment by setting the <strong>ASPNETCORE_ENVIRONMENT</strong> environment variable to <strong>Development</strong>
    and restarting the app.
</p>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Error.cshtml.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace VirtualScrap_25069_24169.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }

}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Index.cshtml`

```html
﻿@page
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}

<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <p>Learn about <a href="https://learn.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Index.cshtml.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VirtualScrap_25069_24169.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Privacy.cshtml`

```html
﻿@page
@model PrivacyModel
@{
    ViewData["Title"] = "Privacy Policy";
}
<h1>@ViewData["Title"]</h1>

<p>Use this page to detail your site's privacy policy.</p>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Privacy.cshtml.cs`

```csharp
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VirtualScrap_25069_24169.Pages
{
    public class PrivacyModel : PageModel
    {
        public void OnGet()
        {
        }
    }

}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\_ViewImports.cshtml`

```html
﻿@using VirtualScrap_25069_24169
@namespace VirtualScrap_25069_24169.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\_ViewStart.cshtml`

```html
﻿@{
    Layout = "_Layout";
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Create.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Categories.CreateModel

        @{
        ViewData["Title"] = "Create";
        }
        
        <h1>Create</h1>
        
    <h4>Category</h4>
    <hr />
    <div class="row">
    <div class="col-md-4">
    <form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <label asp-for="Category.Name" class="control-label"></label>
                <input asp-for="Category.Name" class="form-control" />
                <span asp-validation-for="Category.Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Create.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public CreateModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Delete.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Categories.DeleteModel

@{
    ViewData["Title"] = "Delete";
}

<h1>Delete</h1>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Category</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Category.Name)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Category.Name)
        </dd>
    </dl>
    
    <form method="post">
        <input type="hidden" asp-for="Category.Id" />
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-page="./Index">Back to List</a>
    </form>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Delete.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DeleteModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);

            if (category is not null)
            {
                Category = category;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                Category = category;
                _context.Categories.Remove(Category);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Details.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Categories.DetailsModel

@{
    ViewData["Title"] = "Details";
}

<h1>Details</h1>

<div>
    <h4>Category</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Category.Name)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Category.Name)
        </dd>
    </dl>
</div>
<div>
    <a asp-page="./Edit" asp-route-id="@Model.Category.Id">Edit</a> |
    <a asp-page="./Index">Back to List</a>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Details.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Categories
{
    public class DetailsModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);

            if (category is not null)
            {
                Category = category;

                return Page();
            }

            return NotFound();
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Edit.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Categories.EditModel

@{
    ViewData["Title"] = "Edit";
}

<h1>Edit</h1>

<h4>Category</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <input type="hidden" asp-for="Category.Id" />
            <div class="form-group">
                <label asp-for="Category.Name" class="control-label"></label>
                <input asp-for="Category.Name" class="form-control" />
                <span asp-validation-for="Category.Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Save" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="./Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Edit.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category =  await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            Category = category;
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

            _context.Attach(Category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(Category.Id))
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

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Index.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Categories.IndexModel

@{
    ViewData["Title"] = "Index";
}

<h1>Index</h1>

<p>
    <a asp-page="Create">Create New</a>
</p>
<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Category[0].Name)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
@foreach (var item in Model.Category) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.Name)
            </td>
            <td>
                <a asp-page="./Edit" asp-route-id="@item.Id">Edit</a> |
                <a asp-page="./Details" asp-route-id="@item.Id">Details</a> |
                <a asp-page="./Delete" asp-route-id="@item.Id">Delete</a>
            </td>
        </tr>
}
    </tbody>
</table>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Categories\Index.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public IndexModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Category> Category { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Category = await _context.Categories.ToListAsync();
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Create.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.MyUsers.CreateModel

        @{
        ViewData["Title"] = "Create";
        }
        
        <h1>Create</h1>
        
    <h4>MyUser</h4>
    <hr />
    <div class="row">
    <div class="col-md-4">
    <form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <span class="text-danger">*</span>
                <label asp-for="MyUser.Name" class="control-label"></label>
                <input asp-for="MyUser.Name" class="form-control" aria-required="true"/>
                <span asp-validation-for="MyUser.Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="MyUser.CellPhone" class="control-label"></label>
                <input asp-for="MyUser.CellPhone" class="form-control" />
                <span asp-validation-for="MyUser.CellPhone" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="MyUser.IdUser" class="control-label"></label>
                <input asp-for="MyUser.IdUser" class="form-control" />
                <span asp-validation-for="MyUser.IdUser" class="text-danger"></span>
            </div>
            <div class="form-group form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="MyUser.IsDeleted" /> @Html.DisplayNameFor(model => model.MyUser.IsDeleted)
                </label>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Create.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class CreateModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public CreateModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public MyUser MyUser { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.MyUsers.Add(MyUser);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Delete.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.MyUsers.DeleteModel

@{
    ViewData["Title"] = "Delete";
}

<h1>Delete</h1>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>MyUser</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.Name)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.Name)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.CellPhone)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.CellPhone)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.IdUser)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.IdUser)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.IsDeleted)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.IsDeleted)
        </dd>
    </dl>
    
    <form method="post">
        <input type="hidden" asp-for="MyUser.Id" />
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-page="./Index">Back to List</a>
    </form>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Delete.cshtml.cs`

```csharp
﻿using System;
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

            var myuser = await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == id);

            if (myuser is not null)
            {
                MyUser = myuser;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myuser = await _context.MyUsers.FindAsync(id);
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

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Details.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.MyUsers.DetailsModel

@{
    ViewData["Title"] = "Details";
}

<h1>Details</h1>

<div>
    <h4>MyUser</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.Name)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.Name)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.CellPhone)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.CellPhone)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.IdUser)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.IdUser)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.MyUser.IsDeleted)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.MyUser.IsDeleted)
        </dd>
    </dl>
</div>
<div>
    <a asp-page="./Edit" asp-route-id="@Model.MyUser.Id">Edit</a> |
    <a asp-page="./Index">Back to List</a>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Details.cshtml.cs`

```csharp
﻿using System;
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

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Edit.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.MyUsers.EditModel

@{
    ViewData["Title"] = "Edit";
}

<h1>Edit</h1>

<h4>MyUser</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <input type="hidden" asp-for="MyUser.Id" />
            <div class="form-group">
<span class="text-danger">*</span>
                <label asp-for="MyUser.Name" class="control-label"></label>
                <input asp-for="MyUser.Name" class="form-control" aria-required="true"/>
                <span asp-validation-for="MyUser.Name" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="MyUser.CellPhone" class="control-label"></label>
                <input asp-for="MyUser.CellPhone" class="form-control" />
                <span asp-validation-for="MyUser.CellPhone" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="MyUser.IdUser" class="control-label"></label>
                <input asp-for="MyUser.IdUser" class="form-control" />
                <span asp-validation-for="MyUser.IdUser" class="text-danger"></span>
            </div>
            <div class="form-group form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="MyUser.IsDeleted" /> @Html.DisplayNameFor(model => model.MyUser.IsDeleted)
                </label>
            </div>
            <div class="form-group">
                <input type="submit" value="Save" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="./Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Edit.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.MyUsers
{
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
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

            var myuser =  await _context.MyUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (myuser == null)
            {
                return NotFound();
            }
            MyUser = myuser;
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

            _context.Attach(MyUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MyUserExists(MyUser.Id))
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

        private bool MyUserExists(int id)
        {
            return _context.MyUsers.Any(e => e.Id == id);
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Index.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.MyUsers.IndexModel

@{
    ViewData["Title"] = "Index";
}

<h1>Index</h1>

<p>
    <a asp-page="Create">Create New</a>
</p>
<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.MyUser[0].Name)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.MyUser[0].CellPhone)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.MyUser[0].IdUser)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.MyUser[0].IsDeleted)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
@foreach (var item in Model.MyUser) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.Name)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.CellPhone)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.IdUser)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.IsDeleted)
            </td>
            <td>
                <a asp-page="./Edit" asp-route-id="@item.Id">Edit</a> |
                <a asp-page="./Details" asp-route-id="@item.Id">Details</a> |
                <a asp-page="./Delete" asp-route-id="@item.Id">Delete</a>
            </td>
        </tr>
}
    </tbody>
</table>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\MyUsers\Index.cshtml.cs`

```csharp
﻿using System;
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
    public class IndexModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public IndexModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MyUser> MyUser { get;set; } = default!;

        public async Task OnGetAsync()
        {
            MyUser = await _context.MyUsers.ToListAsync();
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Create.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Posts.CreateModel

        @{
        ViewData["Title"] = "Create";
        }
        
        <h1>Create</h1>
        
    <h4>Post</h4>
    <hr />
    <div class="row">
    <div class="col-md-4">
    <form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <div class="form-group">
                <span class="text-danger">*</span>
                <label asp-for="Post.Title" class="control-label"></label>
                <input asp-for="Post.Title" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Title" class="text-danger"></span>
            </div>
            <div class="form-group">
                <span class="text-danger">*</span>
                <label asp-for="Post.Description" class="control-label"></label>
                <input asp-for="Post.Description" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Description" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Post.CellPhone" class="control-label"></label>
                <input asp-for="Post.CellPhone" class="form-control" />
                <span asp-validation-for="Post.CellPhone" class="text-danger"></span>
            </div>
            <div class="form-group">
                <span class="text-danger">*</span>
                <label asp-for="Post.Photo" class="control-label"></label>
                <input asp-for="Post.Photo" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Photo" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Post.CategoryFK" class="control-label"></label>
                <select asp-for="Post.CategoryFK" class ="form-control" asp-items="ViewBag.CategoryFK" ></select>
            </div>
            <div class="form-group">
                <input type="submit" value="Create" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Create.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class CreateModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public CreateModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["CategoryFK"] = new SelectList(_context.Categories, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Delete.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Posts.DeleteModel

@{
    ViewData["Title"] = "Delete";
}

<h1>Delete</h1>

<h3>Are you sure you want to delete this?</h3>
<div>
    <h4>Post</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Title)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Title)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Description)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Description)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.CellPhone)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.CellPhone)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.PostDate)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.PostDate)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Photo)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Photo)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.PostCategory)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.PostCategory.Id)
        </dd>
    </dl>
    
    <form method="post">
        <input type="hidden" asp-for="Post.Id" />
        <input type="submit" value="Delete" class="btn btn-danger" /> |
        <a asp-page="./Index">Back to List</a>
    </form>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Delete.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class DeleteModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DeleteModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);

            if (post is not null)
            {
                Post = post;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                Post = post;
                _context.Posts.Remove(Post);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Details.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Posts.DetailsModel

@{
    ViewData["Title"] = "Details";
}

<h1>Details</h1>

<div>
    <h4>Post</h4>
    <hr />
    <dl class="row">
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Title)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Title)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Description)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Description)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.CellPhone)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.CellPhone)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.PostDate)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.PostDate)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.Photo)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.Photo)
        </dd>
        <dt class="col-sm-2">
            @Html.DisplayNameFor(model => model.Post.PostCategory)
        </dt>
        <dd class="col-sm-10">
            @Html.DisplayFor(model => model.Post.PostCategory.Id)
        </dd>
    </dl>
</div>
<div>
    <a asp-page="./Edit" asp-route-id="@Model.Post.Id">Edit</a> |
    <a asp-page="./Index">Back to List</a>
</div>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Details.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class DetailsModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public DetailsModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Post Post { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);

            if (post is not null)
            {
                Post = post;

                return Page();
            }

            return NotFound();
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Edit.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Posts.EditModel

@{
    ViewData["Title"] = "Edit";
}

<h1>Edit</h1>

<h4>Post</h4>
<hr />
<div class="row">
    <div class="col-md-4">
        <form method="post">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            <input type="hidden" asp-for="Post.Id" />
            <div class="form-group">
<span class="text-danger">*</span>
                <label asp-for="Post.Title" class="control-label"></label>
                <input asp-for="Post.Title" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Title" class="text-danger"></span>
            </div>
            <div class="form-group">
<span class="text-danger">*</span>
                <label asp-for="Post.Description" class="control-label"></label>
                <input asp-for="Post.Description" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Description" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Post.CellPhone" class="control-label"></label>
                <input asp-for="Post.CellPhone" class="form-control" />
                <span asp-validation-for="Post.CellPhone" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Post.PostDate" class="control-label"></label>
                <input asp-for="Post.PostDate" class="form-control" />
                <span asp-validation-for="Post.PostDate" class="text-danger"></span>
            </div>
            <div class="form-group">
<span class="text-danger">*</span>
                <label asp-for="Post.Photo" class="control-label"></label>
                <input asp-for="Post.Photo" class="form-control" aria-required="true"/>
                <span asp-validation-for="Post.Photo" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="Post.CategoryFK" class="control-label"></label>
                <select asp-for="Post.CategoryFK" class="form-control" asp-items="ViewBag.CategoryFK" ></select>
                <span asp-validation-for="Post.CategoryFK" class="text-danger"></span>
            </div>
            <div class="form-group">
                <input type="submit" value="Save" class="btn btn-primary" />
            </div>
        </form>
    </div>
</div>

<div>
    <a asp-page="./Index">Back to List</a>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Edit.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class EditModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public EditModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Post Post { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post =  await _context.Posts.FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
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

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Index.cshtml`

```html
﻿@page
@model VirtualScrap_25069_24169.Pages.Posts.IndexModel

@{
    ViewData["Title"] = "Index";
}

<h1>Index</h1>

<p>
    <a asp-page="Create">Create New</a>
</p>
<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].Title)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].Description)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].CellPhone)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].PostDate)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].Photo)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.Post[0].PostCategory)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
@foreach (var item in Model.Post) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.Title)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Description)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.CellPhone)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.PostDate)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.Photo)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.PostCategory.Id)
            </td>
            <td>
                <a asp-page="./Edit" asp-route-id="@item.Id">Edit</a> |
                <a asp-page="./Details" asp-route-id="@item.Id">Details</a> |
                <a asp-page="./Delete" asp-route-id="@item.Id">Delete</a>
            </td>
        </tr>
}
    </tbody>
</table>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Posts\Index.cshtml.cs`

```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Pages.Posts
{
    public class IndexModel : PageModel
    {
        private readonly VirtualScrap_25069_24169.Data.ApplicationDbContext _context;

        public IndexModel(VirtualScrap_25069_24169.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Post> Post { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Post = await _context.Posts
                .Include(p => p.PostCategory).ToListAsync();
        }
    }
}

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Shared\_Layout.cshtml`

```html
﻿<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - VirtualScrap_25069_24169</title>
    <script type="importmap"></script>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/VirtualScrap_25069_24169.styles.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
            <div class="container">
                <a class="navbar-brand" asp-area="" asp-page="/Index">VirtualScrap_25069_24169</a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                        aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-page="/Index">Home</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-page="/Privacy">Privacy</a>
                        </li>
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    <div class="container">
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted">
        <div class="container">
            &copy; 2026 - VirtualScrap_25069_24169 - <a asp-area="" asp-page="/Privacy">Privacy</a>
        </div>
    </footer>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

```

## Ficheiro: `VirtualScrap-25069-24169\VirtualScrap-25069-24169\Pages\Shared\_ValidationScriptsPartial.cshtml`

```html
﻿<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js"></script>

```

