using System.Security.Claims;
using Api.Back.Controllers.Projects;
using Api.Back.DTOs.Requests.Projects;
using Api.Back.DTOs.Responses;
using Api.Back.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Controllers.Projects
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectService> _serviceMock;
        private readonly Mock<IValidator<PostProjectRequest>> _validatorMock;
        private readonly ProjectsController _sut;

        public ProjectsControllerTests()
        {
            _serviceMock = new Mock<IProjectService>();
            _validatorMock = new Mock<IValidator<PostProjectRequest>>();
            _sut = new ProjectsController(_serviceMock.Object, _validatorMock.Object);
        }

        private static void SetUser(ControllerBase controller, Guid? userId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private static void SetInvalidGuidClaim(ControllerBase controller)
        {
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "not-a-guid") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private static ProjectResponse CreateProjectResponse(Guid? id = null, Guid? createdById = null) =>
            new(
                id ?? Guid.NewGuid(),
                "Projet Test",
                "Description",
                "#FFF",
                "https://img.com/x.png",
                createdById ?? Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow
            );

        // ---------------- PostProject ----------------

        [Fact]
        public async Task PostProject_Should_ReturnBadRequest_When_ValidationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new PostProjectRequest("", null, null, null);
            var failures = new List<ValidationFailure> { new("Name", "Le nom est obligatoire.") };
            _validatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _sut.PostProject(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _serviceMock.Verify(s => s.PostProjectAsync(It.IsAny<PostProjectRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task PostProject_Should_ReturnCreatedAtAction_When_ValidationSucceeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new PostProjectRequest("Nom", "Description", "#FFF", "https://img.com/x.png");
            var response = CreateProjectResponse();

            _validatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _serviceMock.Setup(s => s.PostProjectAsync(request, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.PostProject(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(ProjectsController.GetProjectById));
            createdResult.RouteValues!["id"].Should().Be(response.Id);
            createdResult.Value.Should().Be(response);

            _serviceMock.Verify(s => s.PostProjectAsync(request, userId), Times.Once);
        }

        // ---------------- ListUserProjects ----------------

        [Fact]
        public async Task ListUserProjects_Should_ReturnOk_With_Projects()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var projects = new List<ProjectResponse> { CreateProjectResponse(), CreateProjectResponse() };
            _serviceMock.Setup(s => s.ListUserProjectsAsync(userId)).ReturnsAsync(projects);

            // Act
            var result = await _sut.ListUserProjects();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(projects);
            _serviceMock.Verify(s => s.ListUserProjectsAsync(userId), Times.Once);
        }

        // ---------------- GetProjectById ----------------

        [Fact]
        public async Task GetProjectById_Should_ReturnNotFound_When_ProjectDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var projectId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetProjectByIdAsync(projectId, userId)).ReturnsAsync((ProjectResponse?)null);

            // Act
            var result = await _sut.GetProjectById(projectId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetProjectById_Should_ReturnOk_When_ProjectExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var projectId = Guid.NewGuid();
            var response = CreateProjectResponse(projectId);
            _serviceMock.Setup(s => s.GetProjectByIdAsync(projectId, userId)).ReturnsAsync(response);

            // Act
            var result = await _sut.GetProjectById(projectId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }

        // ---------------- PutProject ----------------

        [Fact]
        public async Task PutProject_Should_ReturnOk_With_UpdatedProject()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var projectId = Guid.NewGuid();
            var request = new PutProjectRequest("Nom", "Description", "#FFF", "https://img.com/x.png");
            var response = CreateProjectResponse(projectId);

            _serviceMock.Setup(s => s.PutProjectAsync(projectId, request, userId)).ReturnsAsync(response);

            // Act
            var result = await _sut.PutProject(projectId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.PutProjectAsync(projectId, request, userId), Times.Once);
        }

        // ---------------- PatchProject ----------------

        [Fact]
        public async Task PatchProject_Should_ReturnOk_With_UpdatedProject()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var projectId = Guid.NewGuid();
            var request = new PatchProjectRequest(Name: "Nom");
            var response = CreateProjectResponse(projectId);

            _serviceMock.Setup(s => s.PatchProjectAsync(projectId, request, userId)).ReturnsAsync(response);

            // Act
            var result = await _sut.PatchProject(projectId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
            _serviceMock.Verify(s => s.PatchProjectAsync(projectId, request, userId), Times.Once);
        }

        // ---------------- GetCurrentIdentityId (comportement hérité) ----------------

        [Fact]
        public async Task ListUserProjects_Should_ThrowUnauthorizedAccessException_When_NoIdentityClaim()
        {
            // Arrange
            SetUser(_sut, null);

            // Act
            var act = async () => await _sut.ListUserProjects();

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task ListUserProjects_Should_ThrowUnauthorizedAccessException_When_ClaimIsNotValidGuid()
        {
            // Arrange
            SetInvalidGuidClaim(_sut);

            // Act
            var act = async () => await _sut.ListUserProjects();

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}