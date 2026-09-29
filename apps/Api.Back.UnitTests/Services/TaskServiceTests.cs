using Api.Back.DTOs.Requests.Task;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
        private readonly Mock<IProjectMemberRepository> _memberRepositoryMock = new();
        private readonly Mock<ITaskAssignmentService> _assignmentServiceMock = new();
        private readonly TaskService _sut;

        public TaskServiceTests()
        {
            _sut = new TaskService(_taskRepositoryMock.Object, _memberRepositoryMock.Object, _assignmentServiceMock.Object);
        }

        private static DbTask CreateDbTask(
            Guid? id = null,
            Guid? projectId = null,
            Guid? assigneeId = null,
            bool isChecked = false,
            DateTime? closedAt = null,
            DbIdentity? assignee = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            ProjectId = projectId ?? Guid.NewGuid(),
            Name = "Tâche test",
            AssigneeId = assigneeId,
            Assignee = assignee,
            IsChecked = isChecked,
            ClosedAt = closedAt,
            CreatedAt = DateTimeOffset.UtcNow,
            RequiredDomains = new List<string>()
        };

        private static DbIdentity CreateIdentity(string firstName = "Bob", string lastName = "Martin") => new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            EncryptedProfile = [0],
            Title = "Dev",
            Experience = "3",
            PreferenceId = Guid.NewGuid()
        };

        // ------------------- PostAsync -------------------

        [Fact]
        public async Task PostAsync_Should_ThrowUnauthorized_When_UserNotMemberOfProject()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", null, null, null);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.PostAsync(request, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbTask>()), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Should_ThrowUnauthorized_When_ManualAssigneeNotMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", null, null, assigneeId);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(true);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, assigneeId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.PostAsync(request, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("L'utilisateur assigné n'est pas membre du projet.");
            _taskRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbTask>()), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Should_CreateTask_With_ManualAssignment_And_SetTargetWeek()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", "Desc", null, assigneeId);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(true);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, assigneeId))
                .ReturnsAsync(true);

            DbTask? capturedTask = null;
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbTask>()))
                .Callback<DbTask>(t => capturedTask = t)
                .Returns(Task.CompletedTask);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedTask);

            // Act
            var result = await _sut.PostAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.AssigneeId.Should().Be(assigneeId);
            result.TargetWeek.Should().NotBeNull(); // 🌟 doit être fixé même en assignation manuelle

            _assignmentServiceMock.Verify(a => a.CalculateAssignmentAsync(
                It.IsAny<Guid>(), It.IsAny<TaskDifficulty>(), It.IsAny<IReadOnlyList<string>>()), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Should_TriggerAutoAssignment_When_NoAssignee_And_DifficultySet()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var autoAssigneeId = Guid.NewGuid();
            var targetWeek = new DateTime(2026, 9, 21);
            var domains = new List<string> { "React" };
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", null, null, null, TaskDifficulty.Medium, domains);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(true);
            _assignmentServiceMock.Setup(a => a.CalculateAssignmentAsync(request.ProjectId, TaskDifficulty.Medium, domains))
                .ReturnsAsync((autoAssigneeId, targetWeek));

            DbTask? capturedTask = null;
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbTask>()))
                .Callback<DbTask>(t => capturedTask = t)
                .Returns(Task.CompletedTask);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedTask);

            // Act
            var result = await _sut.PostAsync(request, userId);

            // Assert
            result.AssigneeId.Should().Be(autoAssigneeId);
            result.TargetWeek.Should().Be(targetWeek);

            // 🌟 Pas de vérification manuelle d'appartenance pour l'assigné auto (déjà garanti par l'algo)
            _memberRepositoryMock.Verify(m => m.IsMemberAsync(request.ProjectId, autoAssigneeId), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Should_NotTriggerAutoAssignment_When_NoAssignee_And_DifficultyIsNone()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", null, null, null, TaskDifficulty.None);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(true);

            DbTask? capturedTask = null;
            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbTask>()))
                .Callback<DbTask>(t => capturedTask = t)
                .Returns(Task.CompletedTask);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => capturedTask);

            // Act
            var result = await _sut.PostAsync(request, userId);

            // Assert
            result.AssigneeId.Should().BeNull();
            result.TargetWeek.Should().BeNull();

            _assignmentServiceMock.Verify(a => a.CalculateAssignmentAsync(
                It.IsAny<Guid>(), It.IsAny<TaskDifficulty>(), It.IsAny<IReadOnlyList<string>>()), Times.Never);
        }

        [Fact]
        public async Task PostAsync_Should_UseFallback_When_RefreshedTaskIsNull()
        {
            // Arrange — couvre le cas fullTask == null → task.MapToDto()
            var userId = Guid.NewGuid();
            var request = new PostTaskRequest(Guid.NewGuid(), "Nom", null, null, null);

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(request.ProjectId, userId))
                .ReturnsAsync(true);

            _taskRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbTask>()))
                .Returns(Task.CompletedTask);
            _taskRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((DbTask?)null);

            // Act
            var result = await _sut.PostAsync(request, userId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Nom");
        }

        // ------------------- GetTaskByIdAsync -------------------

        [Fact]
        public async Task GetTaskByIdAsync_Should_ReturnNull_When_TaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((DbTask?)null);

            // Act
            var result = await _sut.GetTaskByIdAsync(taskId, userId);

            // Assert
            result.Should().BeNull();
            _memberRepositoryMock.Verify(m => m.IsMemberAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetTaskByIdAsync_Should_ReturnMappedTask_When_UserHasAccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask(assignee: CreateIdentity());

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.GetTaskByIdAsync(task.Id, userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(task.Id);
            result.AssigneeName.Should().Be("Bob Martin");
        }

        [Fact]
        public async Task GetTaskByIdAsync_Should_ThrowUnauthorized_When_UserNotMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask();

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.GetTaskByIdAsync(task.Id, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // ------------------- GetByProjectAsync -------------------

        [Fact]
        public async Task GetByProjectAsync_Should_ReturnMappedTasks_When_UserHasAccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var tasks = new List<DbTask> { CreateDbTask(projectId: projectId), CreateDbTask(projectId: projectId) };

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(projectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.GetByProjectIdAsync(projectId))
                .ReturnsAsync(tasks);

            // Act
            var result = await _sut.GetByProjectAsync(projectId, userId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByProjectAsync_Should_ThrowUnauthorized_When_UserNotMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            _memberRepositoryMock.Setup(m => m.IsMemberAsync(projectId, userId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.GetByProjectAsync(projectId, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _taskRepositoryMock.Verify(r => r.GetByProjectIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        // ------------------- UpdateAsync -------------------

        [Fact]
        public async Task UpdateAsync_Should_ReturnNull_When_TaskDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var request = new UpdateTaskRequest(null, null, null, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId))
                .ReturnsAsync((DbTask?)null);

            // Act
            var result = await _sut.UpdateAsync(taskId, request, userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_ThrowUnauthorized_When_UserNotMemberOfProject()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask();
            var request = new UpdateTaskRequest(null, null, null, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_UnassignTask_When_UnassignTaskIsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var oldAssigneeId = Guid.NewGuid();
            var task = CreateDbTask(assigneeId: oldAssigneeId, assignee: CreateIdentity());
            var request = new UpdateTaskRequest(null, null, null, null, null, null, null, null, UnassignTask: true);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Callback<DbTask>(t => t.Assignee = null)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            result!.AssigneeId.Should().BeNull();
            result.AssigneeName.Should().BeNull();

            _memberRepositoryMock.Verify(m => m.IsMemberAsync(task.ProjectId, It.Is<Guid>(id => id != userId)), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_AssignNewUser_When_ProvidedAndIsMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newAssigneeId = Guid.NewGuid();
            var task = CreateDbTask();
            var request = new UpdateTaskRequest(null, null, null, null, newAssigneeId, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, newAssigneeId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            result!.AssigneeId.Should().Be(newAssigneeId);
        }

        [Fact]
        public async Task UpdateAsync_Should_ThrowUnauthorized_When_NewAssigneeNotMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newAssigneeId = Guid.NewGuid();
            var task = CreateDbTask();
            var request = new UpdateTaskRequest(null, null, null, null, newAssigneeId, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, newAssigneeId))
                .ReturnsAsync(false);

            // Act
            var act = async () => await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("L'utilisateur assigné n'est pas membre du projet.");
            _taskRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DbTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_NotCallIsMemberAsync_When_SameAssigneeIdProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var task = CreateDbTask(assigneeId: assigneeId);
            var request = new UpdateTaskRequest(null, null, null, null, assigneeId, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            _memberRepositoryMock.Verify(m => m.IsMemberAsync(task.ProjectId, assigneeId), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_SetClosedAt_When_IsCheckedBecomesTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask(isChecked: false);
            var request = new UpdateTaskRequest(null, null, true, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            result!.IsChecked.Should().BeTrue();
            task.ClosedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_ClearClosedAt_When_IsCheckedBecomesFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask(isChecked: true, closedAt: DateTime.UtcNow.AddDays(-1));
            var request = new UpdateTaskRequest(null, null, false, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            result!.IsChecked.Should().BeFalse();
            task.ClosedAt.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_NotChangeClosedAt_When_IsCheckedValueUnchanged()
        {
            var userId = Guid.NewGuid();
            var originalClosedAt = DateTime.UtcNow.AddDays(-3);
            var task = CreateDbTask(isChecked: true, closedAt: originalClosedAt);
            var request = new UpdateTaskRequest(null, null, true, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            task.ClosedAt.Should().Be(originalClosedAt);
        }

        [Fact]
        public async Task UpdateAsync_Should_UpdateNameAndDescription_When_Provided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var task = CreateDbTask();
            var request = new UpdateTaskRequest("Nouveau nom", "Nouvelle description", null, null, null, null, null, null);

            _taskRepositoryMock.Setup(r => r.GetByIdAsync(task.Id))
                .ReturnsAsync(task);
            _memberRepositoryMock.Setup(m => m.IsMemberAsync(task.ProjectId, userId))
                .ReturnsAsync(true);
            _taskRepositoryMock.Setup(r => r.UpdateAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateAsync(task.Id, request, userId);

            // Assert
            result!.Name.Should().Be("Nouveau nom");
            result.Description.Should().Be("Nouvelle description");
        }
    }
}