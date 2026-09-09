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
        public TaskService(ITaskRepository taskRepository, IProjectMemberRepository projectMemberRepository)
        {
            _taskRepository = taskRepository;
            _projectMemberRepository = projectMemberRepository;
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

            var task = new DbTask
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                CreatedAt = DateTime.UtcNow,
                IsChecked = false,
                IsArchived = false
            };

            await _taskRepository.AddAsync(task);

            return task.MapToDto();
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

            task.Name = dto.Name ?? task.Name;
            task.Description = dto.Description ?? task.Description;
            task.IsArchived = dto.IsArchived ?? task.IsArchived;
            task.UpdatedAt = DateTime.UtcNow;

            if (dto.IsChecked.HasValue)
            {
                var newCheckedValue = dto.IsChecked.Value;
                if (newCheckedValue && !task.IsChecked)
                {
                    task.ClosedAt = DateTime.UtcNow;
                }
                else if (!newCheckedValue && task.IsChecked)
                {
                    task.ClosedAt = null;
                }

                task.IsChecked = newCheckedValue;
            }
            await _taskRepository.UpdateAsync(task);

            return task.MapToDto();
        }
    }
}