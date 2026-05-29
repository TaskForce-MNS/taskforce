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
        Task<Guid> VerifyAssertionAndLoginAsync(
            AuthenticatorAssertionRawResponse assertionResponse,
            AssertionOptions originalOptions,
            string jwtSecretKey,
            string jwtIssuer,
            string jwtAudience);
        string GenerateJwtToken(Guid identityId, string secretKey, string issuer, string audience, int expirationMinutes);
        Task<(string accessToken, string refreshToken)> GenerateAuthResponseAsync(Guid identityId, string deviceId);
    }

    public class AuthService : IAuthService
    {
        private readonly IFido2 _fido2;
        private readonly IIdentityRepository _identityRepository;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;

        public AuthService(IFido2 fido2, IIdentityRepository identityRepository, IRefreshTokenService refreshTokenService, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(fido2);
            ArgumentNullException.ThrowIfNull(identityRepository);
            ArgumentNullException.ThrowIfNull(refreshTokenService);
            ArgumentNullException.ThrowIfNull(configuration);

            _fido2 = fido2;
            _identityRepository = identityRepository;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
        }

        public CredentialCreateOptions RequestNewCredential(string rpId)
        {
            var user = new Fido2User
            {
                Name = "Anonyme",
                Id = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")),
                DisplayName = "Identité"
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
                FirstName = dto.FirstName,
                LastName = dto.LastName,
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
            return _fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                AllowedCredentials = new List<PublicKeyCredentialDescriptor>(),
                UserVerification = UserVerificationRequirement.Required
            });
        }

        public async Task<Guid> VerifyAssertionAndLoginAsync(
            AuthenticatorAssertionRawResponse assertionResponse,
            AssertionOptions originalOptions,
            string jwtSecretKey,
            string jwtIssuer,
            string jwtAudience)
        {
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
                IsUserHandleOwnerOfCredentialIdCallback = (args, cancellationToken) =>
                {
                    return Task.FromResult(true);
                }
            };

            var assertionResult = await _fido2.MakeAssertionAsync(makeAssertionParams, cancellationToken: default);

            await _identityRepository.UpdateSignatureCounterAsync(credential.DescriptorId, assertionResult.SignCount);

            return identity.Id;
        }
        public string GenerateJwtToken(Guid identityId, string secretKey, string issuer, string audience, int expirationMinutes)
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
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<(string accessToken, string refreshToken)> GenerateAuthResponseAsync(Guid identityId, string deviceId)
        {
            var secretKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key manquant");
            var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer manquant");
            var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience manquant");

            var refreshTokenTtlDays = _configuration.GetValue<int>("Redis:RefreshTokenTtlDays", 30);
            var accessTokenTtlMinutes = _configuration.GetValue<int>("Jwt:AccessTokenTtlMinutes", 60);

            var jwt = GenerateJwtToken(identityId, secretKey, issuer, audience, accessTokenTtlMinutes);

            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(identityId.ToString(), deviceId);
            var expirationTime = DateTime.UtcNow.AddDays(refreshTokenTtlDays);

            await _refreshTokenService.StoreRefreshTokenAsync(identityId.ToString(), deviceId, refreshToken, expirationTime);

            return (jwt, refreshToken);
        }
    }
}
// 🚀 4. Le Cache Distribué (Remplacement du MemoryCache)
// Dans ton AuthController, nous avons utilisé IMemoryCache pour stocker les "Challenges" cryptographiques de FIDO2 pendant 5 minutes.
// C'est parfait pour le développement. Mais en production, si ton architecture s'agrandit, il vaut mieux utiliser Redis (IDistributedCache en .NET). Ainsi, si un utilisateur demande un Challenge FIDO2 au Serveur A, mais que sa réponse est traitée par le Serveur B, le Serveur B trouvera quand même le défi cryptographique dans Redis ! Tu peux aussi l'utiliser pour mettre en cache des requêtes PostgreSQL très lourdes qui ne changent pas souvent (comme des listes de références ou des catalogues).
// 🛡️ 3. Le Rate Limiting (Protection de l'API)
// Pour éviter qu'un robot ou un script malveillant ne spamme ton application (DDoS) ou ne tente d'épuiser tes ressources, tu peux utiliser le middleware de Rate Limiting natif d'ASP.NET Core et le brancher sur Redis.
// Tu pourras définir des règles strictes partagées sur tout ton réseau : "Une même adresse IP ne peut pas appeler /login/options plus de 5 fois par minute". Redis, avec sa vitesse en mémoire, fera ce compte de manière invisible sans jamais ralentir tes vrais utilisateurs.