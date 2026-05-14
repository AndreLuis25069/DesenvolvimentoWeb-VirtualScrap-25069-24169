using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
            .WithMany(u => u.Sent_Comments) 
            .HasForeignKey(c => c.AutorFK)
            .OnDelete(DeleteBehavior.Restrict);

            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um utilizador, os comentarios recebidos pelo mesmo sejam eliminados
            /// em CASCADE.
            /// </summary>
            builder.Entity<Comment>()
            .HasOne(c => c.Recipient)
            .WithMany(u => u.Received_Comments) 
            .HasForeignKey(c => c.RecipientFK)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}