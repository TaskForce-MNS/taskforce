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
        public DbSet<DbProject> Projects { get; set; }
        public DbSet<DbInvitation> Invitations { get; set; }
        public DbSet<DbProjectMember> ProjectMembers { get; set; }
        public DbSet<DbTask> Tasks { get; set; }
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

            modelBuilder.Entity<DbProject>()
                    .HasOne(p => p.CreatedBy)
                    .WithMany()
                    .HasForeignKey(p => p.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DbInvitation>(entity =>
            {
                entity.ToTable("invitations");

                entity.HasOne(e => e.Project)
                .WithMany(p => p.Invitations)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Token).IsUnique();

                entity.HasIndex(e => e.ExpiresAt);
            });
            modelBuilder.Entity<DbProjectMember>(entity =>
            {
                entity.ToTable("project_members");

                entity.HasKey(e => new { e.ProjectId, e.IdentityId });

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.Members)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Identity)
                      .WithMany()
                      .HasForeignKey(e => e.IdentityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Role)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });
            modelBuilder.Entity<DbTask>(entity =>
            {
                entity.ToTable("tasks");
                entity.HasOne(e => e.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.IsChecked)
                      .IsRequired();
            });

        }
    }

}
