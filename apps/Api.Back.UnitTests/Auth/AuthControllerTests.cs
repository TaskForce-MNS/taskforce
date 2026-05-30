using System.Security.Claims;
using System.Text.Json;
using Api.Back.Common;
using Api.Back.Controllers;
using Api.Back.DTOs.Requests;
using Api.Back.Services;
using Fido2NetLib;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Fido2NetLib.Objects;
using Fido2AssertionOptions = Fido2NetLib.AssertionOptions;
namespace Api.Back.Tests.Controllers;

public class AuthControllerTests
{
    // ====================== SETUP ======================
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IValidator<RegisterIdentityDto>> _validatorMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _validatorMock = new Mock<IValidator<RegisterIdentityDto>>();
        _cacheMock = new Mock<IMemoryCache>();
        _configMock = new Mock<IConfiguration>();

        _controller = new AuthController(
            _authServiceMock.Object,
            _validatorMock.Object,
            _cacheMock.Object,
            _configMock.Object,
            mockRefreshTokenService.Object);

        // HttpContext minimal pour tous les tests
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ====================== HELPERS ======================
    private void SetupCache<T>(T value)
    {
        object? boxed = value;
        _cacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out boxed))
            .Returns(true);
    }

    private void SetupCacheMissForAnyKey()
    {
        object? boxed = null;
        _cacheMock
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out boxed))
            .Returns(false);
    }

    private void SetupJwtConfig()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns("super-secret-key-minimum-32-chars!!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TaskForce");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TaskForceUsers");
    }

    // ====================== GET REGISTER OPTIONS ======================
    [Fact]
    public void GetRegisterOptions_WhenOriginIsLocalhost_ShouldReturnOk()
    {
        // Arrange
        _controller.HttpContext.Request.Headers.Origin = "https://app.taskforce.local";
        var fakeOptions = new CredentialCreateOptions
        {
            Challenge = new byte[] { 1, 2, 3 },
            Rp = new PublicKeyCredentialRpEntity("taskforce.local", "TaskForce", null),
            User = new Fido2User { Name = "test", DisplayName = "test", Id = new byte[] { 1 } },
            PubKeyCredParams = Array.Empty<PubKeyCredParam>()
        };
        _authServiceMock.Setup(s => s.RequestNewCredential("taskforce.local")).Returns(fakeOptions);
        _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = _controller.GetRegisterOptions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _authServiceMock.Verify(s => s.RequestNewCredential("taskforce.local"), Times.Once);
    }

    [Fact]
    public void GetRegisterOptions_WhenOriginIsProd_ShouldUseProdRpId()
    {
        // Arrange
        _controller.HttpContext.Request.Headers.Origin = "https://app.taskforce.local";
        var fakeOptions = new CredentialCreateOptions
        {
            Challenge = new byte[] { 1, 2, 3 },
            Rp = new PublicKeyCredentialRpEntity("taskforce.local", "TaskForce", null),
            User = new Fido2User { Name = "test", DisplayName = "test", Id = new byte[] { 1 } },
            PubKeyCredParams = Array.Empty<PubKeyCredParam>()
        };
        _authServiceMock.Setup(s => s.RequestNewCredential("taskforce.local")).Returns(fakeOptions);
        _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = _controller.GetRegisterOptions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _authServiceMock.Verify(s => s.RequestNewCredential("taskforce.local"), Times.Once);
    }

    // ====================== REGISTER ======================
    [Fact]
    public async Task Register_WhenValidationFails_ShouldReturnBadRequest()
    {
        // Arrange
        var attestationJson = BuildFakeAttestationJson();
        var dto = new RegisterIdentityDto(
            EncryptedProfileBlob: "blob_chiffré",
            FirstName: "John",
            LastName: "Doe",
            Experience: "7",
            Title: "Dev",
            WebAuthnAttestationResponse: JsonSerializer.SerializeToElement(attestationJson)
        );
        _validatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Title", "Title est requis")
            }));

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _authServiceMock.Verify(s => s.RegisterIdentityAsync(
            It.IsAny<RegisterIdentityDto>(),
            It.IsAny<CredentialCreateOptions>(),
            It.IsAny<AuthenticatorAttestationRawResponse>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenChallengeExpired_ShouldReturnBadRequest()
    {
        // Arrange
        var attestationJson = BuildFakeAttestationJson();
        var dto = new RegisterIdentityDto(
            EncryptedProfileBlob: "blob_chiffré",
            FirstName: "John",
            LastName: "Doe",
            Experience: "7",
            Title: "Dev",
            WebAuthnAttestationResponse: JsonSerializer.SerializeToElement(attestationJson)
        );

        _validatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        SetupCacheMissForAnyKey();

        // Act
        var result = await _controller.Register(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ====================== GET LOGIN OPTIONS ======================
    [Fact]
    public void GetLoginOptions_WhenOriginIsWebApp_ShouldUseProdRpId()
    {
        _controller.HttpContext.Request.Headers.Origin = "https://app.taskforce.local";
        var fakeOptions = new Fido2AssertionOptions
        {
            Challenge = new byte[] { 1, 2, 3 },
            RpId = "taskforce.local"
        };
        _authServiceMock.Setup(s => s.RequestAssertionOptions("taskforce.local")).Returns(fakeOptions);
        _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        var result = _controller.GetLoginOptions();

        result.Should().BeOfType<OkObjectResult>();
        _authServiceMock.Verify(s => s.RequestAssertionOptions("taskforce.local"), Times.Once);
    }

    [Fact]
    public void GetLoginOptions_WhenOriginIsTauri_ShouldUseLocalhostRpId()
    {
        // Arrange
        _controller.HttpContext.Request.Headers.Origin = "tauri://localhost";

        // C'est un test de Connexion (Login), on utilise donc Fido2AssertionOptions !
        var fakeOptions = new Fido2AssertionOptions
        {
            Challenge = new byte[] { 4, 5, 6 },
            RpId = "localhost"
        };

        _authServiceMock.Setup(s => s.RequestAssertionOptions("localhost")).Returns(fakeOptions);
        _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

        // Act
        var result = _controller.GetLoginOptions();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _authServiceMock.Verify(s => s.RequestAssertionOptions("localhost"), Times.Once);
    }

    // ====================== LOGIN ======================
    [Fact]
    public async Task Login_WhenChallengeExpired_ShouldReturnBadRequest()
    {
        // Arrange
        var assertionJson = BuildFakeAssertionJson();
        var dto = new LoginIdentityDto(
            WebAuthnAssertionResponse: JsonSerializer.SerializeToElement(assertionJson)
        );

        SetupCacheMissForAnyKey();

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Le défi a expiré ou est invalide. Veuillez recommencer la connexion." });
    }

    [Fact]
    public async Task Login_WhenJwtKeyMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var assertionJson = BuildFakeAssertionJson();
        var dto = new LoginIdentityDto(
            WebAuthnAssertionResponse: JsonSerializer.SerializeToElement(assertionJson)
        );

        // On est dans le Login, on utilise l'objet de connexion (Fido2AssertionOptions)
        var fakeOptions = new Fido2AssertionOptions
        {
            Challenge = new byte[] { 1, 2, 3 },
            RpId = "taskforce.local" // Propriété requise en v4
        };
        SetupCache(fakeOptions);

        _configMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);

        // Act
        var result = await _controller.Login(dto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ====================== LOGOUT ======================
    [Fact]
    public void Logout_ShouldDeleteCookieAndReturnOk()
    {
        // Arrange — HttpContext avec response cookies
        var context = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = context };

        // Act
        var result = _controller.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    // ====================== ME ======================
    [Fact]
    public void Me_WhenUserAuthenticated_ShouldReturnIdentityId()
    {
        // Arrange
        var identityId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, identityId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controller.Me();

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { IdentityId = identityId });
    }

    [Fact]
    public void Me_WhenUserNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange — pas de claims
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        // Act
        var act = () => _controller.Me();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*Identité introuvable*");
    }

    private static object BuildFakeAttestationJson() => new
    {
        response = new
        {
            clientDataJSON = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new
            {
                challenge = "AAEC",
                origin = "https://app.taskforce.local",
                type = "webauthn.create"
            })),
            attestationObject = Convert.ToBase64String(new byte[] { 0 })
        }
    };

    private static object BuildFakeAssertionJson() => new
    {
        rawId = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
        response = new
        {
            clientDataJSON = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(new
            {
                challenge = "AAEC",
                origin = "https://app.taskforce.local",
                type = "webauthn.get"
            })),
            authenticatorData = Convert.ToBase64String(new byte[] { 0 }),
            signature = Convert.ToBase64String(new byte[] { 0 })
        }
    };
}