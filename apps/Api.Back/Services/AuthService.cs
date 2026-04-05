using Api.Back.DTOs.Requests;
using Api.Back.Models;
using Api.Back.Repositories;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text;

namespace Api.Back.Services
{
    public interface IAuthService
    {
        CredentialCreateOptions RequestNewCredential(string rpId);
        Task<DbIdentity> RegisterIdentityAsync(
            RegisterIdentityDto dto,
            CredentialCreateOptions originalOptions,
            AuthenticatorAttestationRawResponse attestationResponse);
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
    }

}
