using Api.Back.Data;
using Api.Back.Models;
using Api.Back.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Back.UnitTests.Repositories
{
    public sealed class TaskRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly TaskRepository _sut;

        public TaskRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new AppDbContext(options);
            _sut = new TaskRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private static DbIdentity CreateIdentity(string firstName = "Alice", string lastName = "Dupont")
        {
            var preferenceId = Guid.NewGuid();
            return new DbIdentity
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                EncryptedProfile = [0],
                Title = "Dev",
                Experience = "3",
                PreferenceId = preferenceId,
                Preference = new DbPreference
                {
                    Id = preferenceId,
                    Theme = "Dark",
                    Appearance = "Default",
                    IsAutoTheme = true,
                    FontSize = 14.00m,
                    LetterSpacing = 0.00m,
                }
            };
        }

        // ------------------- GetByProjectIdAsync -------------------

        [Fact]
        public async Task GetByProjectIdAsync_Should_ReturnOnlyNonArchivedTasks_ForGivenProject()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();

            var activeTask = new DbTask { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Active", CreatedAt = DateTimeOffset.UtcNow, IsArchived = false };
            var archivedTask = new DbTask { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Archived", CreatedAt = DateTimeOffset.UtcNow, IsArchived = true };
            var otherProjectTask = new DbTask { Id = Guid.NewGuid(), ProjectId = otherProjectId, Name = "Other project", CreatedAt = DateTimeOffset.UtcNow, IsArchived = false };

            _context.Tasks.AddRange(activeTask, archivedTask, otherProjectTask);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.GetByProjectIdAsync(projectId);

            // Assert
            result.Should().ContainSingle();
            result[0].Id.Should().Be(activeTask.Id);
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_ReturnTasks_OrderedByCreatedAt()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var newest = new DbTask { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Newest", CreatedAt = now };
            var oldest = new DbTask { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Oldest", CreatedAt = now.AddDays(-2) };
            var middle = new DbTask { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Middle", CreatedAt = now.AddDays(-1) };

            _context.Tasks.AddRange(newest, oldest, middle);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.GetByProjectIdAsync(projectId);

            // Assert
            result.Select(t => t.Name).Should().ContainInOrder("Oldest", "Middle", "Newest");
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_IncludeAssignee_When_TaskIsAssigned()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var assignee = CreateIdentity("Chloé", "Bernard");

            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Assignée",
                CreatedAt = DateTimeOffset.UtcNow,
                AssigneeId = assignee.Id
            };

            _context.Identities.Add(assignee);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            _context.ChangeTracker.Clear();

            // Act
            var result = await _sut.GetByProjectIdAsync(projectId);

            // Assert
            result.Should().ContainSingle();
            result[0].Assignee.Should().NotBeNull();
            result[0].Assignee!.FirstName.Should().Be("Chloé");
        }

        [Fact]
        public async Task GetByProjectIdAsync_Should_ReturnEmptyList_When_NoTasksExist()
        {
            // Act
            var result = await _sut.GetByProjectIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeEmpty();
        }

        // ------------------- GetByIdAsync -------------------

        [Fact]
        public async Task GetByIdAsync_Should_ReturnTask_With_Assignee_When_TaskExists()
        {
            // Arrange
            var assignee = CreateIdentity();
            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Test",
                CreatedAt = DateTimeOffset.UtcNow,
                AssigneeId = assignee.Id
            };

            _context.Identities.Add(assignee);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
            _context.ChangeTracker.Clear();

            // Act
            var result = await _sut.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(task.Id);
            result.Assignee.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNull_When_TaskDoesNotExist()
        {
            // Act
            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_IncludeArchivedTasks()
        {
            // Arrange
            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Archivée",
                CreatedAt = DateTimeOffset.UtcNow,
                IsArchived = true
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.GetByIdAsync(task.Id);

            // Assert
            result.Should().NotBeNull();
            result!.IsArchived.Should().BeTrue();
        }

        // ------------------- AddAsync -------------------

        [Fact]
        public async Task AddAsync_Should_PersistTask_ToDatabase()
        {
            // Arrange
            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Nouvelle tâche",
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act
            await _sut.AddAsync(task);

            // Assert
            var savedTask = await _context.Tasks.FindAsync([task.Id], TestContext.Current.CancellationToken);
            savedTask.Should().NotBeNull();
            savedTask!.Name.Should().Be("Nouvelle tâche");
        }

        // ------------------- UpdateAsync -------------------

        [Fact]
        public async Task UpdateAsync_Should_PersistChanges_When_EntityIsTracked()
        {
            // Arrange
            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Nom original",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            task.Name = "Nom modifié";
            task.IsChecked = true;

            // Act
            await _sut.UpdateAsync(task);

            // Assert
            _context.ChangeTracker.Clear();
            var updatedTask = await _context.Tasks.FindAsync([task.Id], TestContext.Current.CancellationToken);
            updatedTask!.Name.Should().Be("Nom modifié");
            updatedTask.IsChecked.Should().BeTrue();
        }
    }
}