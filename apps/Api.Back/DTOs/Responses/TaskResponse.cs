namespace Api.Back.DTOs.Responses
{
    public record TaskResponse(
        Guid Id,
        string Name,
        string? Description,
        bool IsChecked,
        bool IsArchived,
        DateTimeOffset CreatedAt,
        Guid ProjectId
    );
}