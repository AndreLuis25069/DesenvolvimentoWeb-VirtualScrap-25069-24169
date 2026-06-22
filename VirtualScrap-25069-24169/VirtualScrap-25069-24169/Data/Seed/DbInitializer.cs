using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VirtualScrap_25069_24169.Data;
using VirtualScrap_25069_24169.Data.Model;

namespace VirtualScrap_25069_24169.Data.Seed
{
    internal class DbInitializer
    {
        // NOTA: Mudou para 'async Task' para poderes usar 'await' de forma segura no início da app
        internal static async Task Initialize(ApplicationDbContext dbContext)
        {
            ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

            // Garantir que a base de dados é existente
            dbContext.Database.EnsureCreated();

            bool thereIsInsert = false;

            // Se as roles não estiverem criadas, irão ser lançadas automaticamente
            if (!dbContext.Roles.Any())
            {
                await dbContext.Roles.AddRangeAsync(
                    new IdentityRole { Id = "Admin", Name = "Admin", NormalizedName = "Admin" },
                    new IdentityRole { Id = "User", Name = "User", NormalizedName = "User" }
                );
                thereIsInsert = true;
            }

            // Variável auxiliar para guardar os utilizadores do Identity
            var identityUsers = Array.Empty<IdentityUser>();
            var hasher = new PasswordHasher<IdentityUser>();

            // Se a tabela do AspNetUsers estiver vazia irão ser criados os seguintes utilizadores
            if (!dbContext.Users.Any())
            {
                var admin1 = new IdentityUser
                {
                    Id = "admin-guid-1", // GUID estático atribuido
                    UserName = "testarogajovirtual@gmail.com",
                    NormalizedUserName = "TESTAROGAJOVIRTUAL@GMAIL.COM",
                    Email = "testarogajovirtual@gmail.com",
                    NormalizedEmail = "TESTAROGAJOVIRTUAL@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D")
                };
                admin1.PasswordHash = hasher.HashPassword(admin1, "Aa0_aa!");

                var admin2 = new IdentityUser
                {
                    Id = "admin-guid-2",
                    UserName = "andralluis2000@gmail.com",
                    NormalizedUserName = "ANDRALLUIS2000@GMAIL.COM",
                    Email = "andralluis2000@gmail.com",
                    NormalizedEmail = "ANDRALLUIS2000@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D")
                };
                admin2.PasswordHash = hasher.HashPassword(admin2, "Testes123@");


                //Utilizador normal numero 1
                var user3 = new IdentityUser
                {
                    Id = "user-guid-3",
                    UserName = "cliente1@gmail.com",
                    NormalizedUserName = "CLIENTE1@GMAIL.COM",
                    Email = "cliente1@gmail.com",
                    NormalizedEmail = "CLIENTE1@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D")
                };
                user3.PasswordHash = hasher.HashPassword(user3, "Aa0_aa!");

                //Utilizador normal numero 2
                var user4 = new IdentityUser
                {
                    Id = "user-guid-4",
                    UserName = "cliente2@gmail.com",
                    NormalizedUserName = "CLIENTE2@GMAIL.COM",
                    Email = "cliente2@gmail.com",
                    NormalizedEmail = "CLIENTE2@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D")
                };
                user4.PasswordHash = hasher.HashPassword(user4, "Aa0_aa!");

                //Utilizador normal numero 3
                var user5 = new IdentityUser
                {
                    Id = "user-guid-5",
                    UserName = "cliente3@gmail.com",
                    NormalizedUserName = "CLIENTE3@GMAIL.COM",
                    Email = "cliente3@gmail.com",
                    NormalizedEmail = "CLIENTE3@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D")
                };
                user5.PasswordHash = hasher.HashPassword(user5, "Aa0_aa!");

                identityUsers = new[] { admin1, admin2, user3, user4, user5 };
                await dbContext.Users.AddRangeAsync(identityUsers);
                

                //Seed para associar as roles aos utilizadores
                await dbContext.UserRoles.AddRangeAsync(
                    new IdentityUserRole<string> { UserId = admin1.Id, RoleId = "Admin" },
                    new IdentityUserRole<string> { UserId = admin2.Id, RoleId = "Admin" },
                    new IdentityUserRole<string> { UserId = user3.Id, RoleId = "User" },
                    new IdentityUserRole<string> { UserId = user4.Id, RoleId = "User" },
                    new IdentityUserRole<string> { UserId = user5.Id, RoleId = "User" }
                );

                thereIsInsert = true;
            }

            // Seed para adicionar os mesmos utilizadores á tabela MyUsers, que trabalha com as relações do projeto todo
            // Só fazemos se a tabela estiver vazia e se tivermos acabado de criar os logins acima
            if (!dbContext.MyUsers.Any() && identityUsers.Length == 5)
            {
                await dbContext.MyUsers.AddRangeAsync(
                    new MyUser
                    {
                        Name = "Admin VirtualScrap 1",
                        CellPhone = "911111111",
                        Photo = "noImage.jpg",
                        IdUser = identityUsers[0].Id 
                    },
                    new MyUser
                    {
                        Name = "André Moreira Rosa Luís",
                        CellPhone = "922222222",
                        Photo = "noImage.jpg",
                        IdUser = identityUsers[1].Id 
                    },
                     new MyUser
                     {
                         Name = "User Normal1",
                         CellPhone = "922222222",
                         Photo = "noImage.jpg",
                         IdUser = identityUsers[2].Id 
                     },
                      new MyUser
                      {
                          Name = "User Normal2",
                          CellPhone = "922222222",
                          Photo = "noImage.jpg",
                          IdUser = identityUsers[3].Id 
                      },
                      new MyUser
                      {
                          Name = "User Normal3",
                          CellPhone = "922222222",
                          Photo = "noImage.jpg",
                          IdUser = identityUsers[4].Id 
                      }
                );
                thereIsInsert = true;
            }

            // 5. PERSISTIR NA BASE DE DADOS
            try
            {
                if (thereIsInsert)
                {
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}