using brevo_csharp.Api;
using brevo_csharp.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using VirtualScrap_25069_24169.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Configurar o SDK da Brevo
var brevoApiKey = builder.Configuration["BrevoSettings:ApiKey"];
Configuration.Default.ApiKey["api-key"] = brevoApiKey;

// Registar a API de e-mails transacionais como Singleton ou Scoped
builder.Services.AddScoped<TransactionalEmailsApi>();
builder.Services.AddHttpClient<IEmailSender, BrevoEmailSender>();

builder.Services.AddControllers();

// configurar o de uso de 'cookies'
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromSeconds(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();

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

// na segunda secção, adicionar para
// começar a usar, realmente, os 'cookies'
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
