using System.Text.Json;

namespace Api.Back.DTOs.Requests
{
    public record LoginIdentityDto(JsonElement WebAuthnAssertionResponse);
}