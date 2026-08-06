using Api.Back.DTOs.Requests.invitation;
using Api.Back.DTOs.Responses.invitation;
using Api.Back.Services;
using Api.Back.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Back.Controllers
{
    [ApiController]
    [Authorize]
    public class InvitationsController : BaseController
    {
        private readonly IInvitationService _invitationService;

        public InvitationsController(IInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        [HttpPost(BackUrls.ProjectInvitations)]
        [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateInvitation(Guid projectId, [FromBody] CreateInvitationRequest request)
        {
            var response = await _invitationService.CreateInvitationAsync(projectId, GetCurrentIdentityId(), request);
            return CreatedAtAction(nameof(ListInvitations), new { projectId }, response);
        }

        [HttpGet(BackUrls.ProjectInvitations)]
        [ProducesResponseType(typeof(IEnumerable<InvitationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ListInvitations(Guid projectId)
        {
            var invitations = await _invitationService.ListActiveInvitationsAsync(projectId, GetCurrentIdentityId());
            return Ok(invitations);
        }

        [HttpDelete(BackUrls.Invitation)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RevokeInvitation(Guid invitationId)
        {
            await _invitationService.RevokeInvitationAsync(invitationId, GetCurrentIdentityId());
            return NoContent();
        }

        [HttpPost(BackUrls.AcceptInvitation)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            var projectId = await _invitationService.AcceptInvitationAsync(request.Token, GetCurrentIdentityId());
            return Ok(new { Message = "Vous avez rejoint le projet !", ProjectId = projectId });
        }
    }
}