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

namespace Api.Back.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterIdentityDto> _validator;
        private readonly IMemoryCache _cache;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterIdentityDto> validator,
            IMemoryCache cache)
        {
            _authService = authService;
            _validator = validator;
            _cache = cache;
        }

        [HttpPost($"{BackUrls.Register}/options")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get Passkey registration options", Description = "Returns the cryptographic challenge required for Passkey creation.")]
        public IActionResult GetRegisterOptions()
        {
            var origin = Request.Headers.Origin.ToString();
            if (string.IsNullOrEmpty(origin)) origin = Request.Headers.Referer.ToString();
            string rpId = "taskforce.local"; // Valeur par défaut (Prod)

            if (origin.Contains("localhost", StringComparison.OrdinalIgnoreCase) || origin.Contains("tauri", StringComparison.OrdinalIgnoreCase))
            {
                rpId = "localhost";
            }

            var options = _authService.RequestNewCredential(rpId);

            var cacheKey = WebEncoders.Base64UrlEncode(options.Challenge);
            _cache.Set(cacheKey, options, TimeSpan.FromMinutes(5));

            return Ok(options);
        }

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
    }
}