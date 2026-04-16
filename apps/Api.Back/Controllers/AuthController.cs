using Api.Back.DTOs.Requests;
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

namespace Api.Back.Controllers
{
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterIdentityDto> _validator;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterIdentityDto> validator,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _authService = authService;
            _validator = validator;
            _cache = cache;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost($"{BackUrls.Register}/options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get Passkey registration options", Description = "Returns the cryptographic challenge required for Passkey creation.")]
        public IActionResult GetRegisterOptions()
        {
            var origin = Request.Headers.Origin.ToString();
            if (string.IsNullOrEmpty(origin)) origin = Request.Headers.Referer.ToString();
            string rpId = "taskforce.local";

            if (origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) || origin.Contains("tauri", StringComparison.OrdinalIgnoreCase))
            {
                rpId = "localhost";
            }

            var options = _authService.RequestNewCredential(rpId);

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

                var jwtSecret = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clé secrète JWT est introuvable.");
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TaskForce";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "TaskForceUsers";

                var token = _authService.GenerateJwtToken(identityCreated.Id, jwtSecret, jwtIssuer, jwtAudience);

                Response.Cookies.Append(SharedConstants.SessionCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Domain = ".taskforce.local",
                    SameSite = SameSiteMode.None,
                    MaxAge = TimeSpan.FromHours(1),
                    Path = "/"
                });
                return CreatedAtAction(nameof(Register), new { id = identityCreated.Id }, new { Message = "Identité Zéro-Connaissance créée !", IdentityId = identityCreated.Id });
            }
            // Tu pourras recréer tes propres exceptions personnalisées (ex: PublicKeyAlreadyExistsException) plus tard !
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

            // On demande un défi d'Assertion 
            var options = _authService.RequestAssertionOptions(rpId);

            // On stocke le défi dans le cache comme pour l'inscription
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

                var clientDataJsonBytes = assertionResponse.Response.ClientDataJson;
                var clientData = JsonSerializer.Deserialize<JsonElement>(clientDataJsonBytes);

                var challengeBase64Url = clientData.GetProperty("challenge").GetString()
                    ?? throw new InvalidOperationException("Challenge non trouvé dans le clientData");

                var challengeBytes = WebEncoders.Base64UrlDecode(challengeBase64Url);
                var cacheKey = WebEncoders.Base64UrlEncode(challengeBytes);

                if (!_cache.TryGetValue(cacheKey, out AssertionOptions? originalOptions) || originalOptions == null)
                {
                    return BadRequest(new { message = "Le défi a expiré ou est invalide. Veuillez recommencer la connexion." });
                }

                var jwtSecret = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("La clé secrète JWT est introuvable sur le serveur.");
                var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TaskForce";
                var jwtAudience = _configuration["Jwt:Audience"] ?? "TaskForceUsers";

                var token = await _authService.VerifyAssertionAndLoginAsync(
                    assertionResponse,
                    originalOptions,
                    jwtSecret,
                    jwtIssuer,
                    jwtAudience);

                Response.Cookies.Append(SharedConstants.SessionCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    MaxAge = TimeSpan.FromHours(1),
                    Path = "/"
                });
                _cache.Remove(cacheKey);

                return Ok(new
                {
                    Message = "Connexion Zéro-Connaissance réussie !"
                });
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
        public IActionResult Logout()
        {
            Response.Cookies.Delete(SharedConstants.SessionCookieName);
            return Ok(new { Message = "Déconnexion réussie." });
        }

        [HttpGet(BackUrls.Me)]
        public IActionResult Me()
        {
            return Ok(new { IdentityId = GetCurrentIdentityId() });
        }
    }
}