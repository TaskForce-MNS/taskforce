using Api.Back.DTOs.Responses;
using Api.Back.Models;

namespace Api.Back.Extensions
{
    public static class TaskExtensions
    {
        public static TaskResponse MapToDto(this DbTask task)
        {
            return new TaskResponse(
                task.Id,
                task.Name,
                task.Description,
                task.IsChecked,
                task.IsArchived,
                task.CreatedAt,
                task.DueDate,
                task.AssigneeId,
                task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
                task.ProjectId,
                task.Difficulty,
                task.StoryPoints,
                task.TargetWeek,
                task.RequiredDomains ?? []
            );
        }
    }
}