using Api.Back.Data;
using Api.Back.Models;
using Api.Back.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Back.UnitTests.Repositories
{
    public sealed class IdentityRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IdentityRepository _sut;

        public IdentityRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new IdentityRepository(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private static DbIdentity CreateIdentity(
            Guid? id = null,
            bool isDeleted = false,
            string firstName = "John",
            string lastName = "Doe",
            string title = "Dev")
        {
            return new DbIdentity
            {
                Id = id ?? Guid.NewGuid(),
                EncryptedProfile = [1, 2, 3],
                FirstName = firstName,
                LastName = lastName,
                Experience = "5+",
                Title = title,
                IsDeleted = isDeleted
            };
        }

        private static DbUserCredential CreateCredential(Guid identityId, byte[]? descriptorId = null, byte[]? publicKey = null)
        {
            return new DbUserCredential
            {
                Id = Guid.NewGuid(),
                IdentityId = identityId,
                DescriptorId = descriptorId ?? [9, 9, 9],
                PublicKey = publicKey ?? [1, 2, 3],
                UserHandle = [4, 5, 6],
                SignatureCounter = 0
            };
        }

        [Fact]
        public async Task GetByPublicKeyAsync_Should_ReturnNull_When_NoMatchingCredential()
        {
            // Arrange
            var publicKeyBase64 = Convert.ToBase64String([1, 2, 3]);

            // Act
            var result = await _sut.GetByPublicKeyAsync(publicKeyBase64);

            // Assert
            result.Should().BeNull();
        }
        // [Fact]
        // public async Task GetByPublicKeyAsync_Should_ReturnIdentity_When_CredentialMatches()
        // {
        //     // Arrange
        //     var publicKeyBytes = new byte[] { 1, 2, 3 };
        //     var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);

        //     var identity = CreateIdentity();

        //     var credential = CreateCredential(
        //         identity.Id,
        //         publicKey: publicKeyBytes
        //     );

        //     identity.Credentials.Add(credential);

        //     _context.Identities.Add(identity);

        //     await _context.SaveChangesAsync(
        //         TestContext.Current.CancellationToken
        //     );

        //     // Vérification du setup
        //     var credentialCheck = await _context.Credentials
        //         .FirstOrDefaultAsync(
        //             c => c.PublicKey.SequenceEqual(publicKeyBytes),
        //             TestContext.Current.CancellationToken
        //         );

        //     credentialCheck.Should().NotBeNull();
        //     credentialCheck!.IdentityId.Should().Be(identity.Id);

        //     // Act
        //     var result = await _sut.GetByPublicKeyAsync(publicKeyBase64);

        //     // Assert
        //     result.Should().NotBeNull();
        //     result!.Id.Should().Be(identity.Id);
        //     result.Credentials.Should().NotBeEmpty();
        // }

        // ---------------- PublicKeyExistsAsync ----------------

        [Fact]
        public async Task PublicKeyExistsAsync_Should_ReturnFalse_When_NoMatch()
        {
            // Arrange
            var publicKeyBase64 = Convert.ToBase64String([1, 2, 3]);

            // Act
            var exists = await _sut.PublicKeyExistsAsync(publicKeyBase64);

            // Assert
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task PublicKeyExistsAsync_Should_ReturnTrue_When_Match()
        {
            // Arrange
            var publicKeyBytes = new byte[] { 1, 2, 3 };
            var identity = CreateIdentity();
            var credential = CreateCredential(identity.Id, publicKey: publicKeyBytes);

            _context.Identities.Add(identity);
            _context.Credentials.Add(credential);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes);

            // Act
            var exists = await _sut.PublicKeyExistsAsync(publicKeyBase64);

            // Assert
            exists.Should().BeTrue();
        }

        // ---------------- AddAsync ----------------

        [Fact]
        public async Task AddAsync_Should_PersistIdentity()
        {
            // Arrange
            var identity = CreateIdentity();

            // Act
            await _sut.AddAsync(identity);

            // Assert
            var saved = await _context.Identities.FindAsync([identity.Id], TestContext.Current.CancellationToken); ;
            saved.Should().NotBeNull();
            saved!.FirstName.Should().Be(identity.FirstName);
        }

        // ---------------- GetByCredentialIdAsync ----------------

        [Fact]
        public async Task GetByCredentialIdAsync_Should_ReturnNull_When_NoMatch()
        {
            // Act
            var result = await _sut.GetByCredentialIdAsync([9, 9, 9]);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByCredentialIdAsync_Should_ReturnIdentity_When_Match()
        {
            // Arrange
            var descriptorId = new byte[] { 7, 7, 7 };

            var identity = CreateIdentity();
            identity.Id = Guid.NewGuid(); // Toujours forcer un ID propre

            var credential = CreateCredential(identity.Id, descriptorId: descriptorId);

            identity.Credentials.Add(credential);

            _context.Identities.Add(identity);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act - nouvelle instance de tableau, même contenu
            var result = await _sut.GetByCredentialIdAsync([7, 7, 7]);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(identity.Id);
        }

        // ---------------- UpdateSignatureCounterAsync ----------------

        [Fact]
        public async Task UpdateSignatureCounterAsync_Should_DoNothing_When_CredentialDoesNotExist()
        {
            // Act
            var act = async () => await _sut.UpdateSignatureCounterAsync([1, 2, 3], 42);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task UpdateSignatureCounterAsync_Should_UpdateCounter_When_SameByteArrayInstance()
        {
            // Arrange
            var descriptorId = new byte[] { 5, 5, 5 };
            var identity = CreateIdentity();
            var credential = CreateCredential(identity.Id, descriptorId: descriptorId);

            _context.Identities.Add(identity);
            _context.Credentials.Add(credential);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act - même référence d'objet passée
            await _sut.UpdateSignatureCounterAsync(descriptorId, 99);

            // Assert
            var updated = await _context.Credentials.FindAsync([credential.Id], TestContext.Current.CancellationToken); ;
            updated!.SignatureCounter.Should().Be(99);
        }

        [Fact]
        public async Task UpdateSignatureCounterAsync_Should_UpdateCounter_When_DifferentInstanceSameContent()
        {
            // Arrange
            var identity = CreateIdentity();
            var credential = CreateCredential(identity.Id, descriptorId: new byte[] { 5, 5, 5 });

            _context.Identities.Add(identity);
            _context.Credentials.Add(credential);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act - nouvelle instance de tableau avec le même contenu
            await _sut.UpdateSignatureCounterAsync([5, 5, 5], 99);

            // Assert
            var updated = await _context.Credentials.FindAsync([credential.Id], TestContext.Current.CancellationToken); ;
            updated!.SignatureCounter.Should().Be(99,
                "la comparaison de byte[] doit se faire par contenu (SequenceEqual), pas par référence (==)");
        }

        // ---------------- GetUserProfileByIdAsync ----------------

        [Fact]
        public async Task GetUserProfileByIdAsync_Should_ReturnNull_When_IdentityDoesNotExist()
        {
            // Act
            var result = await _sut.GetUserProfileByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserProfileByIdAsync_Should_ReturnNull_When_IdentityIsDeleted()
        {
            // Arrange
            var identity = CreateIdentity(isDeleted: true);
            _context.Identities.Add(identity);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.GetUserProfileByIdAsync(identity.Id, TestContext.Current.CancellationToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserProfileByIdAsync_Should_ReturnMappedDto_When_IdentityExists()
        {
            // Arrange
            var identity = CreateIdentity(firstName: "Alice", lastName: "Martin", title: "Lead Dev");
            _context.Identities.Add(identity);
            await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _sut.GetUserProfileByIdAsync(identity.Id, TestContext.Current.CancellationToken);

            // Assert
            result.Should().NotBeNull();
            result!.FirstName.Should().Be("Alice");
            result.LastName.Should().Be("Martin");
            result.Title.Should().Be("Lead Dev");
        }
    }
}