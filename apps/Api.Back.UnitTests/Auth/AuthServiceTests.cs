using Api.Back.DTOs.Requests;
using Api.Back.Models;
using Api.Back.Repositories;
using Api.Back.Services;
using FluentAssertions;
using Moq;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text.Json;
using Xunit;

namespace Api.Back.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IFido2> _fido2Mock = new();
    private readonly Mock<IIdentityRepository> _identityRepositoryMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _authService = new AuthService(_fido2Mock.Object, _identityRepositoryMock.Object);
    }

    private static CredentialCreateOptions CreateValidOptions()
    {
        return new CredentialCreateOptions
        {
            Rp = new PublicKeyCredentialRpEntity("test-rp", "Test RP"),
            User = new Fido2User
            {
                Id = new byte[] { 1, 2, 3 },
                Name = "test",
                DisplayName = "test"
            },
            Challenge = new byte[] { 1, 2, 3 },

            PubKeyCredParams = new List<PubKeyCredParam>
        {
            new PubKeyCredParam(COSE.Algorithm.ES256, PublicKeyCredentialType.PublicKey)
        }
        };
    }

    [Fact]
    public void RequestNewCredential_Should_ReturnValidOptions()
    {
        var options = CreateValidOptions();

        _fido2Mock
            .Setup(f => f.RequestNewCredential(It.IsAny<RequestNewCredentialParams>()))
            .Returns(options);

        var result = _authService.RequestNewCredential("test-rp");

        result.User.Name.Should().Be("test");
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task RegisterIdentityAsync_Should_ThrowArgumentNullException_When_AnyParameterIsNull(
        bool isDtoNull, bool isOptionsNull, bool isAttestationNull)
    {
        var blob = Convert.ToBase64String("FauxProfilChiffre"u8.ToArray());

        var dto = isDtoNull
            ? null
            : new RegisterIdentityDto(
                blob,
                "John",
                "Doe",
                "Dev",
                "7",
                JsonDocument.Parse("{}").RootElement);

        var options = isOptionsNull ? null : CreateValidOptions();
        var attestation = isAttestationNull ? null : new AuthenticatorAttestationRawResponse();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _authService.RegisterIdentityAsync(dto!, options!, attestation!));

        _identityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbIdentity>()), Times.Never);
    }

    // [Fact]
    // public async Task RegisterIdentityAsync_Should_ThrowInvalidOperationException_When_FidoValidationFails()
    // {
    //     var profileBytes = "MonProfil"u8.ToArray();
    //     var blob = Convert.ToBase64String(profileBytes);
    //     var jsonElement = JsonDocument.Parse("{}").RootElement;

    //     var dto = new RegisterIdentityDto(
    //         blob,
    //         "Junior",
    //         "Dev",
    //         JsonDocument.Parse("{}").RootElement);

    //     var options = CreateValidOptions();
    //     var attestation = new AuthenticatorAttestationRawResponse();

    //     _fido2Mock
    //         .Setup(f => f.MakeNewCredentialAsync(
    //             It.IsAny<MakeNewCredentialParams>(),
    //             It.IsAny<CancellationToken>()))
    //         .ReturnsAsync((RegisteredPublicKeyCredential)null!);

    //     var action = () =>
    //         _authService.RegisterIdentityAsync(dto, options, attestation);

    //     await action.Should()
    //         .ThrowAsync<InvalidOperationException>()
    //         .WithMessage("La validation du Passkey a échoué.");

    //     _identityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbIdentity>()), Times.Never);
    // }

    [Fact]
    public async Task RegisterIdentityAsync_Should_CreateIdentityAndSave_When_ValidationSucceeds()
    {
        var profileBytes = "MonSuperProfilChiffre"u8.ToArray();
        var blob = Convert.ToBase64String(profileBytes);
        var jsonElement = JsonDocument.Parse("{}").RootElement;

        // Ordre CORRECT du record : (EncryptedProfileBlob, FirstName, LastName, Experience, Title, WebAuthnAttestationResponse)
        var dto = new RegisterIdentityDto(blob, "John", "Doe", "8", "Architecte", jsonElement);

        var options = CreateValidOptions();
        var attestation = new AuthenticatorAttestationRawResponse();

        var fidoSuccess = new RegisteredPublicKeyCredential
        {
            Id = new byte[] { 10, 20, 30 },
            PublicKey = new byte[] { 99, 88, 77 },
            User = new Fido2User { Id = new byte[] { 1, 2, 3 } },
            SignCount = 0,
            AaGuid = Guid.NewGuid()
        };

        _fido2Mock
            .Setup(f => f.MakeNewCredentialAsync(
                It.IsAny<MakeNewCredentialParams>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fidoSuccess);

        DbIdentity? saved = null;

        _identityRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DbIdentity>()))
            .Callback<DbIdentity>(i => saved = i)
            .Returns(Task.CompletedTask);

        var result = await _authService.RegisterIdentityAsync(dto, options, attestation);

        result.Should().NotBeNull();
        saved.Should().NotBeNull();

        saved!.Experience.Should().Be("8");
        saved.Title.Should().Be("Architecte");
        saved.EncryptedProfile.Should().BeEquivalentTo(profileBytes);

        saved.Credentials.Should().HaveCount(1);
        saved.Credentials.First().DescriptorId.Should().BeEquivalentTo(fidoSuccess.Id);

        _identityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DbIdentity>()), Times.Once);
    }
}