namespace Api.Back.DTOs.Requests.invitation
{
    public record CreateInvitationRequest(
        int? ExpiresInDays,
        int? UsesLeft
    );
}