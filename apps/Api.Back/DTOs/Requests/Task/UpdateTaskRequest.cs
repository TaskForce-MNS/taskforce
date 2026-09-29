using Api.Back.Models;

namespace Api.Back.DTOs.Requests.Task
{
    public record UpdateTaskRequest(
        string? Name,
        string? Description,
        bool? IsChecked,
        bool? IsArchived,
        Guid? AssigneeId,
        DateTime? DueDate,
        TaskDifficulty? Difficulty,
        IReadOnlyList<string>? RequiredDomains,
        bool UnassignTask = false 
    );
}