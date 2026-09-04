using Api.Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Back.Data
{
    public static class DbSeeder
    {
        public static readonly Guid DemoProjectAlphaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid DemoProjectBetaId = Guid.Parse("22222222-2222-2222-2222-222222222223");

        public static readonly Guid[] DemoProjectIds =
        {
            DemoProjectAlphaId,
            DemoProjectBetaId,
        };

        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Projects.AnyAsync())
            {
                return;
            }

            var alice = CreateDemoIdentity("11111111-1111-1111-1111-111111111111", "Alice", "Dupont", "Product Owner", "5");
            var bob = CreateDemoIdentity("11111111-1111-1111-1111-111111111112", "Bob", "Martin", "Développeur Backend", "3");
            var chloe = CreateDemoIdentity("11111111-1111-1111-1111-111111111113", "Chloé", "Bernard", "Designer UI/UX", "4");
            var david = CreateDemoIdentity("11111111-1111-1111-1111-111111111114", "David", "Leroy", "Développeur Frontend", "2");

            foreach (var (identity, preference) in new[] { alice, bob, chloe, david })
            {
                await context.Preferences.AddAsync(preference);
                await context.Identities.AddAsync(identity);
            }

            var projectAlpha = new DbProject
            {
                Id = DemoProjectAlphaId,
                Name = "Projet Alpha",
                Description = "Projet de démonstration généré automatiquement",
                ColorHex = "#587B7F",
                CreatedById = alice.Identity.Id,
                CreatedAt = DateTime.UtcNow,
            };

            var alphaMembers = new List<DbProjectMember>
            {
                new() { ProjectId = DemoProjectAlphaId, IdentityId = alice.Identity.Id, Role = ProjectMemberRole.Owner,  JoinedAt = DateTime.UtcNow },
                new() { ProjectId = DemoProjectAlphaId, IdentityId = bob.Identity.Id,   Role = ProjectMemberRole.Admin,  JoinedAt = DateTime.UtcNow },
                new() { ProjectId = DemoProjectAlphaId, IdentityId = chloe.Identity.Id, Role = ProjectMemberRole.Member, JoinedAt = DateTime.UtcNow },
            };

            var projectBeta = new DbProject
            {
                Id = DemoProjectBetaId,
                Name = "Projet Beta",
                Description = "Deuxième projet de démonstration",
                ColorHex = "#74394E",
                CreatedById = bob.Identity.Id,
                CreatedAt = DateTime.UtcNow,
            };

            var betaMembers = new List<DbProjectMember>
            {
                new() { ProjectId = DemoProjectBetaId, IdentityId = bob.Identity.Id,   Role = ProjectMemberRole.Owner,  JoinedAt = DateTime.UtcNow },
                new() { ProjectId = DemoProjectBetaId, IdentityId = david.Identity.Id, Role = ProjectMemberRole.Member, JoinedAt = DateTime.UtcNow },
                new() { ProjectId = DemoProjectBetaId, IdentityId = chloe.Identity.Id, Role = ProjectMemberRole.Member, JoinedAt = DateTime.UtcNow },
            };

            var tasks = new List<DbTask>
            {
                new() { Id = Guid.NewGuid(), ProjectId = DemoProjectAlphaId, Name = "Mettre en place le frontend React", Description = "Utiliser Zustand pour le state management", IsChecked = true,  ClosedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ProjectId = DemoProjectAlphaId, Name = "Créer les composants UI",           Description = "Faire le design façon IntelliJ",             IsChecked = false, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ProjectId = DemoProjectBetaId,  Name = "Configurer le backend",             Description = "API .NET + PostgreSQL",                      IsChecked = true,  ClosedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), ProjectId = DemoProjectBetaId,  Name = "Écrire les tests",                  Description = "xUnit + Moq",                                IsChecked = false, CreatedAt = DateTime.UtcNow },
            };

            await context.Projects.AddRangeAsync(projectAlpha, projectBeta);
            await context.ProjectMembers.AddRangeAsync(alphaMembers.Concat(betaMembers));
            await context.Tasks.AddRangeAsync(tasks);

            await context.SaveChangesAsync();
        }

        private static (DbIdentity Identity, DbPreference Preference) CreateDemoIdentity(
            string id, string firstName, string lastName, string title, string experience)
        {
            var preferenceId = Guid.NewGuid();

            var preference = new DbPreference
            {
                Id = preferenceId,
                Theme = "Dark",
                Appearance = "Default",
                IsAutoTheme = true,
                FontSize = 14.00m,
                LetterSpacing = 0.00m,
            };

            var identity = new DbIdentity
            {
                Id = Guid.Parse(id),
                FirstName = firstName,
                LastName = lastName,
                EncryptedProfile = [0],
                Title = title,
                Experience = experience,
                PreferenceId = preferenceId,
            };

            return (identity, preference);
        }
    }
}