using Api.Back.DTOs.Requests;
using Api.Back.Models;
using Api.Back.Repositories;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Back.Services
{
    public interface IAuthService
    {
        CredentialCreateOptions RequestNewCredential(string rpId);
        Task<DbIdentity> RegisterIdentityAsync(
            RegisterIdentityDto dto,
            CredentialCreateOptions originalOptions,
            AuthenticatorAttestationRawResponse attestationResponse);
        AssertionOptions RequestAssertionOptions(string rpId);
        Task<string> VerifyAssertionAndLoginAsync(
            AuthenticatorAssertionRawResponse assertionResponse,
            AssertionOptions originalOptions,
            string jwtSecretKey,
            string jwtIssuer,
            string jwtAudience);
    }

    public class AuthService : IAuthService
    {
        private readonly IFido2 _fido2;
        private readonly IIdentityRepository _identityRepository;

        public AuthService(IFido2 fido2, IIdentityRepository identityRepository)
        {
            ArgumentNullException.ThrowIfNull(fido2);
            ArgumentNullException.ThrowIfNull(identityRepository);

            _fido2 = fido2;
            _identityRepository = identityRepository;
        }

        public CredentialCreateOptions RequestNewCredential(string rpId)
        {
            var user = new Fido2User
            {
                Name = "Anonyme",
                Id = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
                DisplayName = "Identité Zéro-Connaissance"
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = user,
                ExcludeCredentials = Array.Empty<PublicKeyCredentialDescriptor>(),
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None
            });
            options.Rp.Id = rpId;
            return options;
        }

        public async Task<DbIdentity> RegisterIdentityAsync(
            RegisterIdentityDto dto,
            CredentialCreateOptions originalOptions,
            AuthenticatorAttestationRawResponse attestationResponse)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(originalOptions);
            ArgumentNullException.ThrowIfNull(attestationResponse);

            var result = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestationResponse,
                OriginalOptions = originalOptions,
                IsCredentialIdUniqueToUserCallback = async (args, cancellationToken) =>
                {
                    var exists = await _identityRepository.PublicKeyExistsAsync(
                        Convert.ToBase64String(args.CredentialId));
                    return !exists;
                }
            });

            var newIdentity = new DbIdentity
            {
                Experience = dto.Experience,
                Title = dto.Title,
                CurrentWorkload = 0,
                WorkloadPoints = 0,
                EncryptedProfile = Convert.FromBase64String(dto.EncryptedProfileBlob),

                Preference = new DbPreference()
            };

            var newCredential = new DbUserCredential
            {
                DescriptorId = result.Id,
                PublicKey = result.PublicKey,
                UserHandle = result.User.Id,
                SignatureCounter = result.SignCount,
                AaGuid = result.AaGuid,
                Identity = newIdentity
            };

            newIdentity.Credentials.Add(newCredential);

            await _identityRepository.AddAsync(newIdentity);

            return newIdentity;

        }
        public AssertionOptions RequestAssertionOptions(string rpId)
        {
            // Dans la v4, on passe un objet unique de configuration (GetAssertionOptionsParams)
            return _fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = new List<PublicKeyCredentialDescriptor>(),
                UserVerification = UserVerificationRequirement.Required
            });
        }

        public async Task<string> VerifyAssertionAndLoginAsync(
            AuthenticatorAssertionRawResponse assertionResponse,
            AssertionOptions originalOptions,
            string jwtSecretKey,
            string jwtIssuer,
            string jwtAudience)
        {
            // Règle CA1062 : On bloque immédiatement si la requête du client est vide
            ArgumentNullException.ThrowIfNull(assertionResponse);

            var identity = await _identityRepository.GetByCredentialIdAsync(assertionResponse.RawId)
                ?? throw new InvalidOperationException("Aucune identité trouvée pour ce Passkey.");

            var credential = identity.Credentials.First(c => c.DescriptorId.SequenceEqual(assertionResponse.RawId));

            var makeAssertionParams = new MakeAssertionParams
            {
                AssertionResponse = assertionResponse,
                OriginalOptions = originalOptions,
                StoredPublicKey = credential.PublicKey,
                StoredSignatureCounter = credential.SignatureCounter,
                // Dans la v4, le delegate prend "args" ET "cancellationToken"
                IsUserHandleOwnerOfCredentialIdCallback = (args, cancellationToken) =>
                {
                    return Task.FromResult(true);
                }
            };

            var assertionResult = await _fido2.MakeAssertionAsync(makeAssertionParams, cancellationToken: default);

            await _identityRepository.UpdateSignatureCounterAsync(credential.DescriptorId, assertionResult.SignCount);

            return GenerateJwtToken(identity.Id, jwtSecretKey, jwtIssuer, jwtAudience);
        }
        private static string GenerateJwtToken(Guid identityId, string secretKey, string issuer, string audience)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, identityId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
