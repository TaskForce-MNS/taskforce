using Api.Back.Models;

namespace Api.Back.DTOs.Requests.User
{
    public record AddUserSkillRequest(
        string Domain, 
        ExperienceLevel Level
    );
}