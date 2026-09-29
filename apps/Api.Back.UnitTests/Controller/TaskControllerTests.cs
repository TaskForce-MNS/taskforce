using Api.Back.Controllers.Task;
using Api.Back.DTOs.Requests.Task;
using Api.Back.DTOs.Responses;
using Api.Back.Models;
using Api.Back.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Controller
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _serviceMock = new();
        private readonly Mock<IValidator<PostTaskRequest>> _postValidatorMock = new();
        private readonly Mock<IValidator<UpdateTaskRequest>> _updateValidatorMock = new();
        private readonly TaskController _sut;

        public TaskControllerTests()
        {
            _sut = new TaskController(_serviceMock.Object, _postValidatorMock.Object, _updateValidatorMock.Object);
        }

        private static void SetUser(ControllerBase controller, Guid? userId)
        {
            var claims = new List<System.Security.Claims.Claim>();
            if (userId.HasValue)
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.Value.ToString()));
            }

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private static TaskResponse CreateTaskResponse(Guid? id = null) => new(
            id ?? Guid.NewGuid(),
            "Nom tâche",
            "Description",
            false,
            false,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            Guid.NewGuid(),
            TaskDifficulty.Simple,
            1,
            null,
            new List<string>()
        );

        // ------------------- PostTask -------------------

        [Fact]
        public async Task PostTask_Should_ReturnCreatedAtAction_When_ValidationSucceeds()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", "Desc", null, null);
            var response = CreateTaskResponse();

            _postValidatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _serviceMock.Setup(s => s.PostAsync(request, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.PostTask(request);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(TaskController.GetTaskById));
            createdResult.RouteValues!["taskId"].Should().Be(response.Id);
            createdResult.Value.Should().Be(response);

            _serviceMock.Verify(s => s.PostAsync(request, userId), Times.Once);
        }

        [Fact]
        public async Task PostTask_Should_ReturnBadRequest_When_ValidationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new PostTaskRequest(Guid.NewGuid(), "", "Desc", null, null);
            var failures = new List<ValidationFailure> { new("Name", "Le nom est requis.") };

            _postValidatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _sut.PostTask(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            _serviceMock.Verify(s => s.PostAsync(It.IsAny<PostTaskRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        // ------------------- GetTaskById -------------------

        [Fact]
        public async Task GetTaskById_Should_ReturnOk_When_TaskExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            SetUser(_sut, userId);

            var response = CreateTaskResponse(taskId);

            _serviceMock.Setup(s => s.GetTaskByIdAsync(taskId, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.GetTaskById(taskId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }

        [Fact]
        public async Task GetTaskById_Should_ReturnNotFound_When_TaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            SetUser(_sut, userId);

            _serviceMock.Setup(s => s.GetTaskByIdAsync(taskId, userId))
                .ReturnsAsync((TaskResponse?)null);

            // Act
            var result = await _sut.GetTaskById(taskId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // ------------------- ListTasksByProject -------------------

        [Fact]
        public async Task ListTasksByProject_Should_ReturnOk_With_Tasks()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            var tasks = new List<TaskResponse> { CreateTaskResponse(), CreateTaskResponse() };

            _serviceMock.Setup(s => s.GetByProjectAsync(projectId, userId))
                .ReturnsAsync(tasks);

            // Act
            var result = await _sut.ListTasksByProject(projectId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(tasks);
        }

        [Fact]
        public async Task ListTasksByProject_Should_ReturnForbidden_When_UserNotMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            SetUser(_sut, userId);

            _serviceMock.Setup(s => s.GetByProjectAsync(projectId, userId))
                .ThrowsAsync(new UnauthorizedAccessException("Accès refusé."));

            // Act
            var result = await _sut.ListTasksByProject(projectId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        // ------------------- UpdateTask -------------------

        [Fact]
        public async Task UpdateTask_Should_ReturnOk_When_ValidationSucceeds_And_TaskExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new UpdateTaskRequest(
                "Nom modifié", null, true, null, null, null, null, null);
            var response = CreateTaskResponse(taskId);

            _updateValidatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _serviceMock.Setup(s => s.UpdateAsync(taskId, request, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _sut.UpdateTask(taskId, request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(response);
        }

        [Fact]
        public async Task UpdateTask_Should_ReturnBadRequest_When_ValidationFails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new UpdateTaskRequest(
                null, null, null, null, null, null, null, null);
            var failures = new List<ValidationFailure> { new("Name", "Nom invalide.") };

            _updateValidatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _sut.UpdateTask(taskId, request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            _serviceMock.Verify(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateTaskRequest>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTask_Should_ReturnNotFound_When_TaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            SetUser(_sut, userId);

            var request = new UpdateTaskRequest(
                "Nom", null, null, null, null, null, null, null);

            _updateValidatorMock.Setup(v => v.ValidateAsync(request, default))
                .ReturnsAsync(new ValidationResult());
            _serviceMock.Setup(s => s.UpdateAsync(taskId, request, userId))
                .ReturnsAsync((TaskResponse?)null);

            // Act
            var result = await _sut.UpdateTask(taskId, request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}