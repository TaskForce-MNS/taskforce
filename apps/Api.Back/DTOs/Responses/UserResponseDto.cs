namespace Api.Back.DTOs.Responses
{
    public record UserResponseDto(
        Guid Id,
        string Email,
        string FullName,
        string Title,
        decimal Workload,
        string Experience,
        DateTime CreatedAt
    );
}