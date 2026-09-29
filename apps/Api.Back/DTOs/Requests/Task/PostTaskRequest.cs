using Api.Back.Models;

namespace Api.Back.DTOs.Requests.Task
{
    public record PostTaskRequest(
            Guid ProjectId,
            string Name,
            string? Description,
            DateTimeOffset? DueDate,
            Guid? AssigneeId,
            TaskDifficulty Difficulty = TaskDifficulty.None,
            IReadOnlyList<string>? RequiredDomains = null
        );
}