using Api.Back.Common; // 🌟 N'oublie pas l'import pour BackUrls !
using Api.Back.DTOs.Requests.User;
using Api.Back.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Back.Controllers
{
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserProfileService _profileService;

        public UserController(IUserProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpPost(BackUrls.AddUserSkill)]
        public async Task<IActionResult> AddSkill([FromBody] AddUserSkillRequest request)
        {
            var userId = GetCurrentIdentityId();
            var result = await _profileService.AddSkillAsync(userId, request);
            
            return Ok(result);
        }

        [HttpDelete(BackUrls.RemoveUserSkill)]
        public async Task<IActionResult> RemoveSkill(Guid skillId)
        {
            var userId = GetCurrentIdentityId();
            var success = await _profileService.RemoveSkillAsync(userId, skillId);
            
            if (!success) 
            {
                return NotFound(new { message = "Compétence introuvable ou non autorisée." });
            }
            
            return NoContent();
        }
    }
}