using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
        public DbSet<PostComment> PostComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /// <summary>
            /// Metodo para fazer com que no uso de DateTimeNow no projeto, a data seja gerada...
            /// </summary>
            builder.Entity<Comment>()
                .Property(c => c.CommentDate)
                .HasDefaultValueSql("GETDATE()");

            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um utilizador, os comentarios do mesmo efetuados noutros perfis...
            /// </summary>
            builder.Entity<Comment>()
                .HasOne(c => c.Autor)
                .WithMany(u => u.SentComments)
                .HasForeignKey(c => c.AutorFK)
                .OnDelete(DeleteBehavior.NoAction);

            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um utilizador, os comentarios recebidos pelo mesmo sejam eliminados...
            /// </summary>
            builder.Entity<Comment>()
                .HasOne(c => c.Recipient)
                .WithMany(u => u.ReceivedComments)
                .HasForeignKey(c => c.RecipientFK)
                .OnDelete(DeleteBehavior.NoAction);


            ////// CONFIGURAÇÃO POSTS
            builder.Entity<Post>()
                .HasOne(p => p.PostOwner)
                .WithMany()
                .HasForeignKey(p => p.OwnerFK)
                .OnDelete(DeleteBehavior.NoAction);


            /// <summary>
            /// Metodo para fazer com que no momento de remoção de um Post, os comentarios recebidos no mesmo sejam eliminados...
            /// </summary>
            builder.Entity<PostComment>()
                .HasOne(c => c.CommentedPost)
                .WithMany(p => p.Commentaries)
                .HasForeignKey(c => c.PostFK)
                .OnDelete(DeleteBehavior.NoAction);

            /// <summary>
            /// Quando um utilizador que comentou for removido, o programa nao vai eliminar os comentarios que ele fez
            /// </summary>
            builder.Entity<PostComment>()
                .HasOne(c => c.Autor)
                .WithMany()
                .HasForeignKey(c => c.AutorFK)
                .OnDelete(DeleteBehavior.NoAction);


            ////// CONFIGURAÇÃO DOS LIKES
            /// <summary>
            /// Se um post for eliminado os seus likes tambem vão ser
            /// </summary>
            builder.Entity<Like>()
                .HasOne(l => l.LikedPost)
                .WithMany(p => p.LikesList)
                .HasForeignKey(l => l.PostFK)
                .OnDelete(DeleteBehavior.NoAction);

            /// <summary>
            /// Se um utilizador for eliminado os seus likes não vão ser
            /// </summary>
            builder.Entity<Like>()
                .HasOne(l => l.LikeAutor)
                .WithMany(u => u.LikesList)
                .HasForeignKey(l => l.LikeAutorFK)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}