using System.Security.Cryptography;
using Api.Back.DTOs.Requests.invitation;
using Api.Back.DTOs.Responses.invitation;
using Api.Back.Extensions;
using Api.Back.Middleware.Exceptions;
using Api.Back.Models;
using Api.Back.Repositories;

namespace Api.Back.Services
{
    public interface IInvitationService
    {
        Task<InvitationResponse> CreateInvitationAsync(Guid projectId, Guid userId, CreateInvitationRequest request);
        Task<IEnumerable<InvitationResponse>> ListActiveInvitationsAsync(Guid projectId, Guid userId);
        Task RevokeInvitationAsync(Guid invitationId, Guid userId);
        Task<Guid> AcceptInvitationAsync(string token, Guid userId);
    }

    public class InvitationService : IInvitationService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IProjectMemberRepository _memberRepository;

        public InvitationService(IInvitationRepository invitationRepository, IProjectMemberRepository memberRepository)
        {
            _invitationRepository = invitationRepository;
            _memberRepository = memberRepository;
        }

        public async Task<InvitationResponse> CreateInvitationAsync(Guid projectId, Guid userId, CreateInvitationRequest request)
        {
            await EnsureIsAdminAsync(projectId, userId);

            var invitation = new DbInvitation
            {
                ProjectId = projectId,
                CreatedById = userId,
                Token = GenerateToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(request.ExpiresInDays ?? 7),
                UsesLeft = request.UsesLeft
            };

            await _invitationRepository.AddAsync(invitation);
            return invitation.ToResponse();
        }

        public async Task<IEnumerable<InvitationResponse>> ListActiveInvitationsAsync(Guid projectId, Guid userId)
        {
            await EnsureIsAdminAsync(projectId, userId);
            var invitations = await _invitationRepository.GetActiveByProjectAsync(projectId);
            return invitations.Select(i => i.ToResponse());
        }

        public async Task RevokeInvitationAsync(Guid invitationId, Guid userId)
        {
            var invitation = await _invitationRepository.GetByIdAsync(invitationId)
                ?? throw new InvitationNotFoundException();

            await EnsureIsAdminAsync(invitation.ProjectId, userId);
            await _invitationRepository.DeleteAsync(invitation);
        }

        public async Task<Guid> AcceptInvitationAsync(string token, Guid userId)
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token)
                ?? throw new InvitationNotFoundException();

            if (invitation.ExpiresAt < DateTime.UtcNow)
                throw new InvitationExpiredException();

            if (invitation.UsesLeft is <= 0)
                throw new InvitationExhaustedException();

            var existingMembership = await _memberRepository.GetAsync(invitation.ProjectId, userId);
            if (existingMembership != null)
                throw new AlreadyProjectMemberException();

            await _memberRepository.AddAsync(new DbProjectMember
            {
                ProjectId = invitation.ProjectId,
                IdentityId = userId,
                Role = ProjectMemberRole.Member
            });

            if (invitation.UsesLeft.HasValue)
            {
                invitation.UsesLeft -= 1;
                await _invitationRepository.UpdateAsync(invitation);
            }
            return invitation.ProjectId;
        }

        private async Task EnsureIsAdminAsync(Guid projectId, Guid userId)
        {
            var membership = await _memberRepository.GetAsync(projectId, userId);
            if (membership == null || membership.Role == ProjectMemberRole.Member)
                throw new NotProjectAdminException();
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}