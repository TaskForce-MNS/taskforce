using Api.Back.Data;
using Api.Back.Models;
using Api.Back.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Back.UnitTests.Repositories
{
    public sealed class InvitationRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly InvitationRepository _sut;

        public InvitationRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new InvitationRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private static DbInvitation CreateInvitation(
            Guid? projectId = null,
            string token = "token-123",
            DateTime? expiresAt = null,
            int? usesLeft = null) => new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId ?? Guid.NewGuid(),
                Token = token,
                CreatedById = Guid.NewGuid(),
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7),
                UsesLeft = usesLeft,
                CreatedAt = DateTime.UtcNow
            };

        // ------------------- AddAsync -------------------

        [Fact]
        public async Task AddAsync_Should_PersistInvitation()
        {
            var invitation = CreateInvitation();

            await _sut.AddAsync(invitation);

            var saved = await _context.Invitations.FindAsync(new object?[] { invitation.Id }, TestContext.Current.CancellationToken);
            saved.Should().NotBeNull();
            saved!.Token.Should().Be("token-123");
        }

        // ------------------- GetByIdAsync -------------------

        [Fact]
        public async Task GetByIdAsync_Should_ReturnInvitation_When_Exists()
        {
            var invitation = CreateInvitation();
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetByIdAsync(invitation.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(invitation.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_ReturnNull_When_NotFound()
        {
            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        // ------------------- GetByTokenAsync -------------------

        [Fact]
        public async Task GetByTokenAsync_Should_ReturnInvitation_When_TokenMatches()
        {
            var invitation = CreateInvitation(token: "unique-token");
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetByTokenAsync("unique-token");

            result.Should().NotBeNull();
            result!.Id.Should().Be(invitation.Id);
        }

        [Fact]
        public async Task GetByTokenAsync_Should_ReturnNull_When_TokenDoesNotMatch()
        {
            var invitation = CreateInvitation(token: "some-token");
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetByTokenAsync("wrong-token");

            result.Should().BeNull();
        }

        // ------------------- GetActiveByProjectAsync -------------------

        [Fact]
        public async Task GetActiveByProjectAsync_Should_ExcludeExpiredInvitations()
        {
            var projectId = Guid.NewGuid();
            var active = CreateInvitation(projectId, "active-token", expiresAt: DateTime.UtcNow.AddDays(1));
            var expired = CreateInvitation(projectId, "expired-token", expiresAt: DateTime.UtcNow.AddDays(-1));

            _context.Invitations.AddRange(active, expired);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetActiveByProjectAsync(projectId);

            result.Should().ContainSingle(i => i.Token == "active-token");
        }

        [Fact]
        public async Task GetActiveByProjectAsync_Should_ExcludeExhaustedInvitations()
        {
            var projectId = Guid.NewGuid();
            var withUsesLeft = CreateInvitation(projectId, "has-uses", usesLeft: 3);
            var exhausted = CreateInvitation(projectId, "no-uses", usesLeft: 0);
            var unlimited = CreateInvitation(projectId, "unlimited", usesLeft: null);

            _context.Invitations.AddRange(withUsesLeft, exhausted, unlimited);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetActiveByProjectAsync(projectId);

            result.Select(i => i.Token).Should().BeEquivalentTo(["has-uses", "unlimited"]);
        }

        [Fact]
        public async Task GetActiveByProjectAsync_Should_ExcludeInvitationsFromOtherProjects()
        {
            var projectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();

            var ownInvitation = CreateInvitation(projectId, "own-token");
            var otherInvitation = CreateInvitation(otherProjectId, "other-token");

            _context.Invitations.AddRange(ownInvitation, otherInvitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var result = await _sut.GetActiveByProjectAsync(projectId);

            result.Should().ContainSingle(i => i.Token == "own-token");
        }

        [Fact]
        public async Task GetActiveByProjectAsync_Should_ReturnEmpty_When_NoActiveInvitations()
        {
            var result = await _sut.GetActiveByProjectAsync(Guid.NewGuid());

            result.Should().BeEmpty();
        }

        // ------------------- UpdateAsync -------------------

        [Fact]
        public async Task UpdateAsync_Should_PersistChanges()
        {
            var invitation = CreateInvitation(usesLeft: 5);
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            invitation.UsesLeft = 4;

            await _sut.UpdateAsync(invitation);

            _context.ChangeTracker.Clear();
            var updated = await _context.Invitations.FindAsync([invitation.Id], TestContext.Current.CancellationToken);
            updated!.UsesLeft.Should().Be(4);
        }

        // ------------------- DeleteAsync -------------------

        [Fact]
        public async Task DeleteAsync_Should_RemoveInvitation()
        {
            var invitation = CreateInvitation();
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            await _sut.DeleteAsync(invitation);

            var result = await _context.Invitations.FindAsync(new object?[] { invitation.Id }, TestContext.Current.CancellationToken);
            result.Should().BeNull();
        }
    }
}