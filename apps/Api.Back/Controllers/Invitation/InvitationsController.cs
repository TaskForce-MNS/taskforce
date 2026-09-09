using Api.Back.DTOs.Requests.invitation;
using Api.Back.DTOs.Responses.invitation;
using Api.Back.Services;
using Api.Back.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Back.Middleware.Exceptions;

namespace Api.Back.Controllers.Invitation
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

        [HttpPost(BackUrls.JoinOrganization)]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> JoinOrganization([FromBody] JoinOrganizationRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(new { message = "Le code d'invitation est requis." });

            try
            {
                var identityId = GetCurrentIdentityId();

                var projectId = await _invitationService.AcceptInvitationAsync(dto.Token, identityId);

                return Ok(new { Message = "Vous avez rejoint le projet avec succès !", ProjectId = projectId });
            }
            catch (InvitationNotFoundException)
            {
                return BadRequest(new { message = "Le lien ou code d'invitation est invalide." });
            }
            catch (InvitationExpiredException)
            {
                return BadRequest(new { message = "Cette invitation a expiré." });
            }
            catch (InvitationExhaustedException)
            {
                return BadRequest(new { message = "Cette invitation a déjà été utilisée son nombre maximum de fois." });
            }
            catch (AlreadyProjectMemberException)
            {
                return BadRequest(new { message = "Vous êtes déjà membre de ce projet." });
            }
            catch
            {
                throw;
            }
        }
    }
}