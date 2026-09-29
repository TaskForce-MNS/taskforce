using Api.Back.DTOs.Requests.Task;
using Api.Back.DTOs.Responses;
using Api.Back.Extensions;
using Api.Back.Models;
using Api.Back.Repositories;

namespace Api.Back.Services
{
    public interface ITaskService
    {
        Task<TaskResponse> PostAsync(PostTaskRequest dto, Guid userId);
        Task<TaskResponse?> GetTaskByIdAsync(Guid taskId, Guid userId);
        Task<List<TaskResponse>> GetByProjectAsync(Guid projectId, Guid userId);
        Task<TaskResponse?> UpdateAsync(Guid taskId, UpdateTaskRequest dto, Guid userId);
    }

    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly ITaskAssignmentService _assignmentService;

        public TaskService(
            ITaskRepository taskRepository,
            IProjectMemberRepository projectMemberRepository,
            ITaskAssignmentService assignmentService)
        {
            _taskRepository = taskRepository;
            _projectMemberRepository = projectMemberRepository;
            _assignmentService = assignmentService;
        }

        private async Task EnsureUserHasAccessToProjectAsync(Guid projectId, Guid userId)
        {
            var isMember = await _projectMemberRepository.IsMemberAsync(projectId, userId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException("Accès refusé : Vous n'êtes pas membre de ce projet.");
            }
        }

        public async Task<TaskResponse> PostAsync(PostTaskRequest dto, Guid userId)
        {
            await EnsureUserHasAccessToProjectAsync(dto.ProjectId, userId);

            Guid? finalAssigneeId = dto.AssigneeId;
            DateTime? finalTargetWeek = null;

            if (finalAssigneeId.HasValue)
            {
                var isAssigneeMember = await _projectMemberRepository.IsMemberAsync(dto.ProjectId, finalAssigneeId.Value);
                if (!isAssigneeMember) throw new UnauthorizedAccessException("L'utilisateur assigné n'est pas membre du projet.");

                var safeDate = dto.DueDate?.UtcDateTime.AddHours(12) ?? DateTime.UtcNow;
                finalTargetWeek = safeDate.GetMondayOfWeek().Date;
            }

            else if (dto.Difficulty != TaskDifficulty.None)
            {
                var domainsList = dto.RequiredDomains?.ToList() ?? new List<string>();

                var (autoAssigneeId, targetWeek) = await _assignmentService.CalculateAssignmentAsync(
                    dto.ProjectId,
                    dto.Difficulty,
                    domainsList);

                finalAssigneeId = autoAssigneeId;
                finalTargetWeek = targetWeek;
            }

            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                DueDate = dto.DueDate,
                AssigneeId = finalAssigneeId,
                CreatedAt = DateTime.UtcNow,
                IsChecked = false,
                IsArchived = false,

                Difficulty = dto.Difficulty,
                StoryPoints = dto.Difficulty.ToStoryPoints(),
                RequiredDomains = dto.RequiredDomains?.ToList() ?? new List<string>(),
                TargetWeek = finalTargetWeek
            };

            await _taskRepository.AddAsync(task);
            var fullTask = await _taskRepository.GetByIdAsync(task.Id);
            return fullTask?.MapToDto() ?? task.MapToDto();
        }

        public async Task<TaskResponse?> GetTaskByIdAsync(Guid taskId, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task == null) return null;
            await EnsureUserHasAccessToProjectAsync(task.ProjectId, userId);

            return task.MapToDto();
        }

        public async Task<List<TaskResponse>> GetByProjectAsync(Guid projectId, Guid userId)
        {
            await EnsureUserHasAccessToProjectAsync(projectId, userId);
            var tasks = await _taskRepository.GetByProjectIdAsync(projectId);

            return tasks.Select(t => t.MapToDto()).ToList();
        }

        public async Task<TaskResponse?> UpdateAsync(Guid taskId, UpdateTaskRequest dto, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return null;

            await EnsureUserHasAccessToProjectAsync(task.ProjectId, userId);
            if (dto.UnassignTask)
            {
                task.AssigneeId = null;
            }
            else if (dto.AssigneeId.HasValue && dto.AssigneeId != task.AssigneeId)
            {
                var isAssigneeMember = await _projectMemberRepository.IsMemberAsync(task.ProjectId, dto.AssigneeId.Value);
                if (!isAssigneeMember) throw new UnauthorizedAccessException("L'utilisateur assigné n'est pas membre du projet.");
                task.AssigneeId = dto.AssigneeId.Value;
            }
            task.Name = dto.Name ?? task.Name;
            task.Description = dto.Description ?? task.Description;
            task.IsArchived = dto.IsArchived ?? task.IsArchived;
            task.DueDate = dto.DueDate ?? task.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            if (dto.IsChecked.HasValue)
            {
                var newCheckedValue = dto.IsChecked.Value;
                if (newCheckedValue && !task.IsChecked) task.ClosedAt = DateTime.UtcNow;
                else if (!newCheckedValue && task.IsChecked) task.ClosedAt = null;

                task.IsChecked = newCheckedValue;
            }

            await _taskRepository.UpdateAsync(task);
            return task.MapToDto();
        }
    }
}