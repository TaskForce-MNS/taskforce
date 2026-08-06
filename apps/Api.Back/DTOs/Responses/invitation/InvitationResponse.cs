namespace Api.Back.DTOs.Responses.invitation
{
    public record InvitationResponse(
        Guid Id,
        Guid ProjectId,
        string Token,
        DateTime ExpiresAt,
        int? UsesLeft,
        DateTime CreatedAt
    );
}