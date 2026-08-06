using Api.Back.DTOs.Requests.Projects;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Services;
using Api.Back.Middleware.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Services.Project
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _repositoryMock;
        private readonly ProjectService _sut; // System Under Test
        private readonly Mock<IProjectMemberRepository> _memberRepositoryMock;

        public ProjectServiceTests()
        {
            _repositoryMock = new Mock<IProjectRepository>();
            _memberRepositoryMock = new Mock<IProjectMemberRepository>();
            _sut = new ProjectService(_repositoryMock.Object, _memberRepositoryMock.Object);
        }

        private static DbProject CreateDbProject(
            Guid? id = null,
            Guid? createdById = null,
            string name = "Projet existant",
            string? description = "Description existante",
            string? colorHex = "#FFF",
            string? imageUrl = "https://example.com/old.png")
        {

            var projectId = id ?? Guid.NewGuid();
            var ownerId = createdById ?? Guid.NewGuid();

            var project = new DbProject
            {
                Id = projectId,
                Name = name,
                Description = description,
                ColorHex = colorHex,
                ImageUrl = imageUrl,
                CreatedById = ownerId
            };


            project.Members.Add(new DbProjectMember
            {
                ProjectId = projectId,
                IdentityId = ownerId,
                Role = ProjectMemberRole.Owner
            });

            return project;
        }
        // ---------------- PostProjectAsync ----------------

        [Fact]
        public async Task PostProjectAsync_Should_CreateProject_And_ReturnMappedResponse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new PostProjectRequest("Nouveau Projet", "Une description", "#FFF", "https://img.com/x.png");

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<DbProject>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PostProjectAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(request.Name);

            _repositoryMock.Verify(r => r.AddAsync(It.Is<DbProject>(p =>
                p.Name == request.Name &&
                p.Description == request.Description &&
                p.ColorHex == request.ColorHex &&
                p.ImageUrl == request.ImageUrl &&
                p.CreatedById == userId
            )), Times.Once);
        }

        // ---------------- GetProjectByIdAsync ----------------

        [Fact]
        public async Task GetProjectByIdAsync_Should_ReturnNull_When_ProjectDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DbProject?)null);

            // Act
            var result = await _sut.GetProjectByIdAsync(Guid.NewGuid(), userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_Should_ReturnNull_When_UserIsNotOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var project = CreateDbProject(createdById: ownerId);

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            // Act
            var result = await _sut.GetProjectByIdAsync(project.Id, requesterId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_Should_ReturnResponse_When_UserIsOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var project = CreateDbProject(createdById: userId);

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            // Act
            var result = await _sut.GetProjectByIdAsync(project.Id, userId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(project.Name);
        }

        // ---------------- ListUserProjectsAsync ----------------

        [Fact]
        public async Task ListUserProjectsAsync_Should_ReturnEmptyList_When_NoProjectsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByUserAsync(userId)).ReturnsAsync(new List<DbProject>());

            // Act
            var result = await _sut.ListUserProjectsAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ListUserProjectsAsync_Should_ReturnMappedProjects_When_ProjectsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projects = new List<DbProject>
            {
                CreateDbProject(createdById: userId, name: "Projet A"),
                CreateDbProject(createdById: userId, name: "Projet B")
            };

            _repositoryMock.Setup(r => r.GetByUserAsync(userId)).ReturnsAsync(projects);

            // Act
            var result = (await _sut.ListUserProjectsAsync(userId)).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Select(p => p.Name).Should().Contain(["Projet A", "Projet B"]);
        }

        // ---------------- PutProjectAsync ----------------

        [Fact]
        public async Task PutProjectAsync_Should_ReplaceAllFields_And_UpdateProject()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var project = CreateDbProject(createdById: userId, name: "Ancien nom");
            var request = new PutProjectRequest("Nouveau nom", "Nouvelle description", "#000", "https://img.com/new.png");

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DbProject>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PutProjectAsync(project.Id, request, userId);

            // Assert
            result.Name.Should().Be(request.Name);
            result.Description.Should().Be(request.Description);
            result.ColorHex.Should().Be(request.ColorHex);
            result.ImageUrl.Should().Be(request.ImageUrl);

            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<DbProject>(p =>
                p.Name == request.Name &&
                p.Description == request.Description &&
                p.ColorHex == request.ColorHex &&
                p.ImageUrl == request.ImageUrl
            )), Times.Once);
        }

        [Fact]
        public async Task PutProjectAsync_Should_ThrowProjectNotFoundException_When_ProjectDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new PutProjectRequest("Nom", "Description", "#FFF", "https://img.com/x.png");

            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DbProject?)null);

            // Act
            var act = async () => await _sut.PutProjectAsync(Guid.NewGuid(), request, userId);

            // Assert
            await act.Should().ThrowAsync<ProjectNotFoundException>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DbProject>()), Times.Never);
        }

        [Fact]
        public async Task PutProjectAsync_Should_ThrowProjectForbiddenException_When_UserIsNotOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var project = CreateDbProject(createdById: ownerId);
            var request = new PutProjectRequest("Nom", "Description", "#FFF", "https://img.com/x.png");

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            // Act
            var act = async () => await _sut.PutProjectAsync(project.Id, request, requesterId);

            // Assert
            await act.Should().ThrowAsync<ProjectForbiddenException>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DbProject>()), Times.Never);
        }

        // ---------------- PatchProjectAsync ----------------

        [Fact]
        public async Task PatchProjectAsync_Should_UpdateOnlyProvidedFields()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var project = CreateDbProject(
                createdById: userId,
                name: "Ancien nom",
                description: "Ancienne description",
                colorHex: "#000",
                imageUrl: "https://img.com/old.png");

            var request = new PatchProjectRequest(Name: "Nouveau nom"); // seul Name est fourni

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DbProject>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PatchProjectAsync(project.Id, request, userId);

            // Assert
            result.Name.Should().Be("Nouveau nom");
            result.Description.Should().Be("Ancienne description"); // inchangé
            result.ColorHex.Should().Be("#000"); // inchangé
            result.ImageUrl.Should().Be("https://img.com/old.png"); // inchangé
        }

        [Fact]
        public async Task PatchProjectAsync_Should_KeepAllExistingValues_When_RequestIsEmpty()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var project = CreateDbProject(createdById: userId, name: "Nom original");
            var request = new PatchProjectRequest(); // tout est null

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DbProject>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.PatchProjectAsync(project.Id, request, userId);

            // Assert
            result.Name.Should().Be("Nom original");
        }

        [Fact]
        public async Task PatchProjectAsync_Should_ThrowProjectNotFoundException_When_ProjectDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new PatchProjectRequest(Name: "Nom");

            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DbProject?)null);

            // Act
            var act = async () => await _sut.PatchProjectAsync(Guid.NewGuid(), request, userId);

            // Assert
            await act.Should().ThrowAsync<ProjectNotFoundException>();
        }

        [Fact]
        public async Task PatchProjectAsync_Should_ThrowProjectForbiddenException_When_UserIsNotOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var requesterId = Guid.NewGuid();
            var project = CreateDbProject(createdById: ownerId);
            var request = new PatchProjectRequest(Name: "Nom");

            _repositoryMock.Setup(r => r.GetByIdAsync(project.Id)).ReturnsAsync(project);

            // Act
            var act = async () => await _sut.PatchProjectAsync(project.Id, request, requesterId);

            // Assert
            await act.Should().ThrowAsync<ProjectForbiddenException>();
        }
    }
}