using System.Text.Json;

namespace Api.Back.DTOs.Requests
{
    public record RegisterIdentityDto(
        string EncryptedProfileBlob,
        string FirstName,
        string LastName,
        string Experience,
        string Title,
        JsonElement WebAuthnAttestationResponse
   );

}