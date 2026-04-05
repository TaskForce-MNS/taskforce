using System.Text.Json;

namespace Api.Back.DTOs.Requests
{
    public record RegisterIdentityDto(
       string EncryptedProfileBlob,
       string Experience,
       string Title,
       JsonElement WebAuthnAttestationResponse
   );

}