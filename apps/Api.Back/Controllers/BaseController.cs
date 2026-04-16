using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

using Api.Back.Common;
namespace Api.Back.Controllers
{
    [ApiController]
    [Authorize]
    // [Route($"{BackUrls.BasePath}/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected Guid GetCurrentIdentityId()
        {
            var userIdString = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                throw new UnauthorizedAccessException("Accès refusé : Identité introuvable dans le token.");
            }

            return userId;
        }
    }
}
