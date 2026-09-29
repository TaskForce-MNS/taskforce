using Api.Back.Data;
using Api.Back.DTOs.Requests.invitation;
using Api.Back.Middleware.Exceptions;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Api.Back.UnitTests.Services
{
    public sealed class InvitationServiceTests : IDisposable
    {
        private readonly Mock<IInvitationRepository> _invitationRepositoryMock = new();
        private readonly Mock<IProjectMemberRepository> _memberRepositoryMock = new();
        private readonly InvitationService _sut;
        private readonly AppDbContext _dummyContext;

        public InvitationServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dummyContext = new AppDbContext(options);

            _sut = new InvitationService(_invitationRepositoryMock.Object, _memberRepositoryMock.Object, _dummyContext);
        }

        public void Dispose()
        {
            _dummyContext.Dispose();
            GC.SuppressFinalize(this);
        }

        private static DbProjectMember CreateMembership(Guid projectId, Guid userId, ProjectMemberRole role) => new()
        {
            ProjectId = projectId,
            IdentityId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        private static DbInvitation CreateInvitation(
            Guid? id = null,
            Guid? projectId = null,
            string token = "valid-token",
            DateTime? expiresAt = null,
            int? usesLeft = null) => new()
            {
                Id = id ?? Guid.NewGuid(),
                ProjectId = projectId ?? Guid.NewGuid(),
                Token = token,
                CreatedById = Guid.NewGuid(),
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
                UsesLeft = usesLeft,
                CreatedAt = DateTime.UtcNow
            };

        // ------------------- CreateInvitationAsync -------------------

        [Fact]
        public async Task CreateInvitationAsync_Should_CreateInvitation_When_UserIsAdmin()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateInvitationRequest(7, 5);

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Admin));

            _invitationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbInvitation>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateInvitationAsync(projectId, userId, request);

            // Assert
            result.Should().NotBeNull();
            result.ProjectId.Should().Be(projectId);
            result.UsesLeft.Should().Be(5);
            result.Token.Should().NotBeNullOrEmpty();

            _invitationRepositoryMock.Verify(r => r.AddAsync(It.Is<DbInvitation>(i =>
                i.ProjectId == projectId &&
                i.CreatedById == userId &&
                i.UsesLeft == 5
            )), Times.Once);
        }

        [Fact]
        public async Task CreateInvitationAsync_Should_UseDefaultExpiration_When_ExpiresInDaysIsNull()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateInvitationRequest(null, null);

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Owner));

            DbInvitation? capturedInvitation = null;
            _invitationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<DbInvitation>()))
                .Callback<DbInvitation>(i => capturedInvitation = i)
                .Returns(Task.CompletedTask);

            // Act
            await _sut.CreateInvitationAsync(projectId, userId, request);

            // Assert
            capturedInvitation.Should().NotBeNull();
            capturedInvitation!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
            capturedInvitation.UsesLeft.Should().BeNull();
        }

        [Fact]
        public async Task CreateInvitationAsync_Should_ThrowNotProjectAdminException_When_UserIsMember()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateInvitationRequest(7, null);

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Member));

            // Act
            var act = async () => await _sut.CreateInvitationAsync(projectId, userId, request);

            // Assert
            await act.Should().ThrowAsync<NotProjectAdminException>();
            _invitationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbInvitation>()), Times.Never);
        }

        [Fact]
        public async Task CreateInvitationAsync_Should_ThrowNotProjectAdminException_When_UserIsNotMember()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = new CreateInvitationRequest(7, null);

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync((DbProjectMember?)null);

            // Act
            var act = async () => await _sut.CreateInvitationAsync(projectId, userId, request);

            // Assert
            await act.Should().ThrowAsync<NotProjectAdminException>();
        }

        // ------------------- ListActiveInvitationsAsync -------------------

        [Fact]
        public async Task ListActiveInvitationsAsync_Should_ReturnMappedInvitations_When_UserIsAdmin()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Admin));

            var invitations = new List<DbInvitation> { CreateInvitation(projectId: projectId), CreateInvitation(projectId: projectId) };
            _invitationRepositoryMock.Setup(r => r.GetActiveByProjectAsync(projectId))
                .ReturnsAsync(invitations);

            // Act
            var result = await _sut.ListActiveInvitationsAsync(projectId, userId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ListActiveInvitationsAsync_Should_ThrowNotProjectAdminException_When_UserIsMember()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Member));

            // Act
            var act = async () => await _sut.ListActiveInvitationsAsync(projectId, userId);

            // Assert
            await act.Should().ThrowAsync<NotProjectAdminException>();
        }

        // ------------------- RevokeInvitationAsync -------------------

        [Fact]
        public async Task RevokeInvitationAsync_Should_DeleteInvitation_When_UserIsAdmin()
        {
            // Arrange
            var invitationId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var invitation = CreateInvitation(id: invitationId, projectId: projectId);

            _invitationRepositoryMock.Setup(r => r.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);
            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Admin));
            _invitationRepositoryMock.Setup(r => r.DeleteAsync(invitation))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.RevokeInvitationAsync(invitationId, userId);

            // Assert
            _invitationRepositoryMock.Verify(r => r.DeleteAsync(invitation), Times.Once);
        }

        [Fact]
        public async Task RevokeInvitationAsync_Should_ThrowInvitationNotFoundException_When_InvitationDoesNotExist()
        {
            // Arrange
            var invitationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            _invitationRepositoryMock.Setup(r => r.GetByIdAsync(invitationId))
                .ReturnsAsync((DbInvitation?)null);

            // Act
            var act = async () => await _sut.RevokeInvitationAsync(invitationId, userId);

            // Assert
            await act.Should().ThrowAsync<InvitationNotFoundException>();
            _memberRepositoryMock.Verify(m => m.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task RevokeInvitationAsync_Should_ThrowNotProjectAdminException_When_UserIsMember()
        {
            // Arrange
            var invitationId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var invitation = CreateInvitation(id: invitationId, projectId: projectId);

            _invitationRepositoryMock.Setup(r => r.GetByIdAsync(invitationId))
                .ReturnsAsync(invitation);
            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Member));

            // Act
            var act = async () => await _sut.RevokeInvitationAsync(invitationId, userId);

            // Assert
            await act.Should().ThrowAsync<NotProjectAdminException>();
            _invitationRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<DbInvitation>()), Times.Never);
        }

        // ------------------- AcceptInvitationAsync -------------------

        [Fact]
        public async Task AcceptInvitationAsync_Should_AddMembership_And_DecrementUsesLeft_When_LimitedUses()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var invitation = CreateInvitation(projectId: projectId, usesLeft: 3);

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token))
                .ReturnsAsync(invitation);
            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync((DbProjectMember?)null);
            _memberRepositoryMock.Setup(m => m.AddAsync(It.IsAny<DbProjectMember>()))
                .Returns(Task.CompletedTask);
            _invitationRepositoryMock.Setup(r => r.UpdateAsync(invitation))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AcceptInvitationAsync(invitation.Token, userId);

            // Assert
            result.Should().Be(projectId);
            invitation.UsesLeft.Should().Be(2);

            _memberRepositoryMock.Verify(m => m.AddAsync(It.Is<DbProjectMember>(pm =>
                pm.ProjectId == projectId &&
                pm.IdentityId == userId &&
                pm.Role == ProjectMemberRole.Member
            )), Times.Once);
            _invitationRepositoryMock.Verify(r => r.UpdateAsync(invitation), Times.Once);
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_NotCallUpdate_When_UsesLeftIsUnlimited()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var invitation = CreateInvitation(projectId: projectId, usesLeft: null);

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token))
                .ReturnsAsync(invitation);
            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync((DbProjectMember?)null);
            _memberRepositoryMock.Setup(m => m.AddAsync(It.IsAny<DbProjectMember>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.AcceptInvitationAsync(invitation.Token, userId);

            // Assert
            result.Should().Be(projectId);
            _invitationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DbInvitation>()), Times.Never);
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_ThrowInvitationNotFoundException_When_TokenDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync("invalid-token"))
                .ReturnsAsync((DbInvitation?)null);

            // Act
            var act = async () => await _sut.AcceptInvitationAsync("invalid-token", userId);

            // Assert
            await act.Should().ThrowAsync<InvitationNotFoundException>();
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_ThrowInvitationExpiredException_When_Expired()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invitation = CreateInvitation(expiresAt: DateTime.UtcNow.AddDays(-1));

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token))
                .ReturnsAsync(invitation);

            // Act
            var act = async () => await _sut.AcceptInvitationAsync(invitation.Token, userId);

            // Assert
            await act.Should().ThrowAsync<InvitationExpiredException>();
            _memberRepositoryMock.Verify(m => m.AddAsync(It.IsAny<DbProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_ThrowInvitationExhaustedException_When_NoUsesLeft()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invitation = CreateInvitation(usesLeft: 0);

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token))
                .ReturnsAsync(invitation);

            // Act
            var act = async () => await _sut.AcceptInvitationAsync(invitation.Token, userId);

            // Assert
            await act.Should().ThrowAsync<InvitationExhaustedException>();
            _memberRepositoryMock.Verify(m => m.AddAsync(It.IsAny<DbProjectMember>()), Times.Never);
        }

        [Fact]
        public async Task AcceptInvitationAsync_Should_ThrowAlreadyProjectMemberException_When_UserAlreadyMember()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var projectId = Guid.NewGuid();
            var invitation = CreateInvitation(projectId: projectId, usesLeft: 5);

            _invitationRepositoryMock.Setup(r => r.GetByTokenAsync(invitation.Token))
                .ReturnsAsync(invitation);
            _memberRepositoryMock.Setup(m => m.GetAsync(projectId, userId))
                .ReturnsAsync(CreateMembership(projectId, userId, ProjectMemberRole.Member));

            // Act
            var act = async () => await _sut.AcceptInvitationAsync(invitation.Token, userId);

            // Assert
            await act.Should().ThrowAsync<AlreadyProjectMemberException>();
            _memberRepositoryMock.Verify(m => m.AddAsync(It.IsAny<DbProjectMember>()), Times.Never);
            _invitationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DbInvitation>()), Times.Never);
        }
    }
}