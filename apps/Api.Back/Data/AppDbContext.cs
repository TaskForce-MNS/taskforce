using Microsoft.EntityFrameworkCore;
using Api.Back.Models;

namespace Api.Back.Data
{
    public class AppDbContext : DbContext
    {
        // Le constructeur permet à .NET de passer la configuration (ex: la chaîne de connexion)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Voici tes 3 tables officielles
        public DbSet<Preference> Preferences { get; set; }
        public DbSet<DbUser> Users { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }

    }

}
