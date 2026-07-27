using Api.Back.DTOs.Requests;
using Api.Back.DTOs.Responses;
using Api.Back.Services;
using Api.Back.Common;
using Api.Back.Tools;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Api.Back.Controllers
{
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterIdentityDto> _validator;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterIdentityDto> validator,
            IMemoryCache cache,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService)
        {
            _authService = authService;
            _validator = validator;
            _cache = cache;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
        }

        [AllowAnonymous]
        [HttpPost($"{BackUrls.Register}/options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get Passkey registration options", Description = "Returns the cryptographic challenge required for Passkey creation.")]
        public IActionResult GetRegisterOptions([FromQuery] string? displayName)
        {
            var origin = Request.Headers.Origin.ToString();
            if (string.IsNullOrEmpty(origin)) origin = Request.Headers.Referer.ToString();
            string rpId = "taskforce.local";

            if (origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) || origin.Contains("tauri", StringComparison.OrdinalIgnoreCase))
            {
                rpId = "localhost";
            }

            var options = _authService.RequestNewCredential(rpId, displayName);

            var cacheKey = WebEncoders.Base64UrlEncode(options.Challenge);
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return Ok(options);
        }

        [AllowAnonymous]
        [HttpPost(BackUrls.Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [SwaggerOperation(Summary = "Register a new Zero-Knowledge identity", Description = "Verifies the Passkey signature and saves the encrypted identity.")]
        public async Task<IActionResult> Register([FromBody] RegisterIdentityDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(dto.WebAuthnAttestationResponse.GetRawText());

                if (attestationResponse == null)
                    return BadRequest(new { message = "Réponse WebAuthn invalide." });

                var clientDataJsonBytes = attestationResponse.Response.ClientDataJson;
                var clientData = JsonSerializer.Deserialize<JsonElement>(clientDataJsonBytes);

                var challengeBase64Url = clientData.GetProperty("challenge").GetString()
                    ?? throw new InvalidOperationException("Challenge non trouvé dans le clientData");

                var challengeBytes = WebEncoders.Base64UrlDecode(challengeBase64Url);

                var cacheKey = WebEncoders.Base64UrlEncode(challengeBytes);
                if (!_cache.TryGetValue(cacheKey, out CredentialCreateOptions? originalOptions) || originalOptions == null)
                {
                    return BadRequest(new { message = "Le défi a expiré ou est invalide. Veuillez recommencer l'inscription." });
                }

                var identityCreated = await _authService.RegisterIdentityAsync(dto, originalOptions, attestationResponse);
                _cache.Remove(cacheKey);

                var deviceId = GetOrCreateDeviceId();
                var (jwt, refreshToken) = await _authService.GenerateAuthResponseAsync(identityCreated.Id, deviceId);

                SetAuthCookies(jwt, refreshToken, deviceId);

                return CreatedAtAction(nameof(Register), new { id = identityCreated.Id }, new { Message = "Identité créée !", IdentityId = identityCreated.Id });
            }
            catch (Exception ex) when (ex.GetType() != typeof(OutOfMemoryException))
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost($"{BackUrls.Login}/options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get Passkey login options", Description = "Returns the cryptographic challenge required for Passkey authentication.")]
        public IActionResult GetLoginOptions()
        {
            var origin = Request.Headers.Origin.ToString();
            if (string.IsNullOrEmpty(origin)) origin = Request.Headers.Referer.ToString();

            string rpId = "taskforce.local";

            if (origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                origin.Contains("tauri", StringComparison.OrdinalIgnoreCase))
            {
                rpId = "localhost";
            }

            var options = _authService.RequestAssertionOptions(rpId);

            var cacheKey = WebEncoders.Base64UrlEncode(options.Challenge);
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return Ok(options);
        }

        [AllowAnonymous]
        [HttpPost(BackUrls.Login)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Login via Passkey", Description = "Verifies the Passkey signature and issues a JWT token.")]
        public async Task<IActionResult> Login([FromBody] LoginIdentityDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            try
            {
                var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(dto.WebAuthnAssertionResponse.GetRawText());

                if (assertionResponse == null)
                    return BadRequest(new { message = "Réponse WebAuthn invalide." });

                var clientData = JsonSerializer.Deserialize<JsonElement>(assertionResponse.Response.ClientDataJson);

                var challengeBase64Url = clientData.GetProperty("challenge").GetString()
                    ?? throw new InvalidOperationException("Challenge non trouvé");

                var cacheKey = WebEncoders.Base64UrlEncode(WebEncoders.Base64UrlDecode(challengeBase64Url));

                if (!_cache.TryGetValue(cacheKey, out AssertionOptions? originalOptions) || originalOptions == null)
                {
                    return BadRequest(new { message = "Le défi a expiré ou est invalide. Veuillez recommencer la connexion." });
                }

                var jwtSecret = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clé secrète JWT est introuvable sur le serveur.");
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TaskForce";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "TaskForceUsers";

                var identityId = await _authService.VerifyAssertionAndLoginAsync(
                    assertionResponse, originalOptions, jwtSecret, jwtIssuer, jwtAudience);

                _cache.Remove(cacheKey);

                var deviceId = GetOrCreateDeviceId();
                var (jwt, refreshToken) = await _authService.GenerateAuthResponseAsync(identityId, deviceId);

                SetAuthCookies(jwt, refreshToken, deviceId);

                return Ok(new { Message = "Connexion réussie !" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.GetType() != typeof(OutOfMemoryException))
            {
                return BadRequest(new { message = "Erreur inattendue lors de la connexion." });
            }
        }

        [HttpPost(BackUrls.Logout)]
        [SwaggerOperation(Summary = "Logout and revoke session")]
        public async Task<IActionResult> Logout()
        {
            var deviceId = Request.Cookies[SharedConstants.DeviceIdCookieName];
            var identityId = GetCurrentIdentityId();

            if (!string.IsNullOrEmpty(deviceId) && identityId != Guid.Empty)
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(identityId.ToString(), deviceId);
            }

            ClearAuthCookies();
            return Ok(new { Message = "Déconnexion réussie." });
        }

        [HttpGet(BackUrls.Me)]
        public async Task<ActionResult<UserResponseDto>> Me(CancellationToken cancellationToken)
        {
            var identityId = GetCurrentIdentityId();

            var userProfile = await _authService.GetUserProfileAsync(identityId, cancellationToken);

            if (userProfile is null)
            {
                return NotFound("Utilisateur introuvable en base de données.");
            }

            return Ok(userProfile);
        }
        
        private string GetOrCreateDeviceId()
        {
            if (Request.Cookies.TryGetValue(SharedConstants.DeviceIdCookieName, out var deviceId) && !string.IsNullOrEmpty(deviceId))
                return deviceId;

            return Guid.NewGuid().ToString("N");
        }

        private void SetAuthCookies(string jwt, string refreshToken, string deviceId)
        {
            var domain = ".taskforce.local";
            var maxAgeRefresh = TimeSpan.FromDays(_configuration.GetValue("Redis:RefreshTokenTtlDays", 30));

            Response.Cookies.Append(SharedConstants.SessionCookieName, jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Domain = domain,
                Path = "/",
                MaxAge = maxAgeRefresh
            });

            // 2. Le Refresh Token
            Response.Cookies.Append(SharedConstants.RefreshTokenCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Domain = domain,
                Path = "/",
                MaxAge = maxAgeRefresh
            });

            // 3. Le Device ID 
            Response.Cookies.Append(SharedConstants.DeviceIdCookieName, deviceId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Domain = domain,
                Path = "/",
                MaxAge = TimeSpan.FromDays(365 * 2)
            });
        }

        private void ClearAuthCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Domain = ".taskforce.local",
                Path = "/",
                MaxAge = TimeSpan.Zero
            };

            Response.Cookies.Delete(SharedConstants.SessionCookieName, cookieOptions);
            Response.Cookies.Delete(SharedConstants.RefreshTokenCookieName, cookieOptions);

            // On ne supprime pas le DeviceId pour le reconnaître à sa prochaine connexion !
        }

        [AllowAnonymous]
        [HttpPost(BackUrls.Refresh)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Refresh JWT Token", Description = "Uses the HttpOnly refresh token cookie to issue a new session.")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[SharedConstants.RefreshTokenCookieName];
            var deviceId = Request.Cookies[SharedConstants.DeviceIdCookieName];
            var expiredJwt = Request.Cookies[SharedConstants.SessionCookieName];

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(deviceId) || string.IsNullOrEmpty(expiredJwt))
            {
                return Unauthorized(new { message = "Session invalide ou expirée." });
            }

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(expiredJwt);
                var identityIdClaim = token.Claims.FirstOrDefault(c => c.Type == "sub");

                if (identityIdClaim == null || !Guid.TryParse(identityIdClaim.Value, out var identityId))
                {
                    return Unauthorized(new { message = "IdentityId invalide." });
                }

                var isValid = await _refreshTokenService.ValidateRefreshTokenAsync(identityId.ToString(), deviceId, refreshToken);
                if (!isValid)
                {
                    ClearAuthCookies();
                    return Unauthorized(new { message = "Refresh token invalide ou expiré." });
                }

                await _refreshTokenService.RevokeRefreshTokenAsync(identityId.ToString(), deviceId);
                var (newAccessToken, newRefreshToken) = await _authService.GenerateAuthResponseAsync(identityId, deviceId);

                SetAuthCookies(newAccessToken, newRefreshToken, deviceId);

                return Ok(new { Message = "Session rafraîchie avec succès." });
            }
            catch (SecurityTokenException)
            {
                ClearAuthCookies();
                return Unauthorized("Token invalide ou expiré.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}