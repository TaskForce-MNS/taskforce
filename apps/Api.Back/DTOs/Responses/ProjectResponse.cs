namespace Api.Back.DTOs.Responses
{
    public record ProjectResponse(
        Guid Id,
        string Name,
        string? Description,
        string? ColorHex,
        string? ImageUrl,
        Guid CreatedById,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string CurrentUserRole
    );

}
