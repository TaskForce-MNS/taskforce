using Microsoft.EntityFrameworkCore;
using Api.Back.Models;

namespace Api.Back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<DbIdentity> Identities { get; set; }
        public DbSet<DbUserCredential> Credentials { get; set; }
        public DbSet<DbPreference> Preferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DbIdentity>()
                .HasOne(i => i.Preference)
                .WithOne(p => p.Identity)
                .HasForeignKey<DbIdentity>(i => i.PreferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DbIdentity>()
                .HasMany(i => i.Credentials)
                .WithOne(c => c.Identity)
                .HasForeignKey(c => c.IdentityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
