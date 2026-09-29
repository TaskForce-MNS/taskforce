using Api.Back.Models;

namespace Api.Back.DTOs.Responses
{
    public record UserResponseDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Title,
        decimal CurrentWorkload,
        string Experience,
        DateTime CreatedAt,
        IReadOnlyList<UserSkillDto> Skills
    );
    public record UserSkillDto(Guid Id, string Domain, ExperienceLevel Level);
}