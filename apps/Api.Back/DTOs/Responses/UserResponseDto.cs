namespace Api.Back.DTOs.Responses
{
    public record UserResponseDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Title,
        decimal CurrentWorkload,
        string Experience,
        DateTime CreatedAt
    );
}