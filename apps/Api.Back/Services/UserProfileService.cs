using Api.Back.DTOs.Requests.User;
using Api.Back.DTOs.Responses;
using Api.Back.Models;
using Api.Back.Repositories;

namespace Api.Back.Services
{
    public interface IUserProfileService
    {
        Task<UserSkillDto> AddSkillAsync(Guid userId, AddUserSkillRequest dto);
        Task<bool> RemoveSkillAsync(Guid userId, Guid skillId);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IIdentityRepository _repository;

        public UserProfileService(IIdentityRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserSkillDto> AddSkillAsync(Guid userId, AddUserSkillRequest dto)
        {
            var skill = new DbUserSkill
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Domain = dto.Domain.Trim(),
                Level = dto.Level
            };

            await _repository.AddSkillAsync(skill);

            return new UserSkillDto(skill.Id, skill.Domain, skill.Level);
        }

        public async Task<bool> RemoveSkillAsync(Guid userId, Guid skillId)
        {
            var skill = await _repository.GetSkillByIdAsync(skillId, userId);

            if (skill == null) return false;

            await _repository.RemoveSkillAsync(skill);
            return true;
        }
    }
}