using Api.Back.DTOs.Responses.invitation;
using Api.Back.Models;

namespace Api.Back.Extensions
{
    public static class InvitationExtensions
    {
        public static InvitationResponse ToResponse(this DbInvitation invitation) => new(
            invitation.Id,
            invitation.ProjectId,
            invitation.Token,
            invitation.ExpiresAt,
            invitation.UsesLeft,
            invitation.CreatedAt
        );
    }
}