using Api.Back.Models;

namespace Api.Back.DTOs.Responses
{
    public record TaskResponse(
        Guid Id,
        string Name,
        string? Description,
        bool IsChecked,
        bool IsArchived,
        DateTimeOffset CreatedAt,
        DateTimeOffset? DueDate,
        Guid? AssigneeId,
        string? AssigneeName,
        Guid ProjectId,
        TaskDifficulty Difficulty,
        int StoryPoints,
        DateTime? TargetWeek,
        IReadOnlyList<string> RequiredDomains
    );
}