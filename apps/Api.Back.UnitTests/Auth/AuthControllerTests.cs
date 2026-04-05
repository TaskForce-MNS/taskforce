using Api.Back.Controllers;
using Api.Back.DTOs.Requests;
using Api.Back.Services;
using Api.Back.Models;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Fido2NetLib;
using Fido2NetLib.Objects;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Api.Back.UnitTests.Auth
{
    public sealed class AuthControllerTests : IDisposable
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IValidator<RegisterIdentityDto>> _validatorMock;
        private readonly MemoryCache _realCache;
        private readonly AuthController _controller;
        private readonly DefaultHttpContext _httpContext;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _validatorMock = new Mock<IValidator<RegisterIdentityDto>>();

            _realCache = new MemoryCache(new MemoryCacheOptions());

            _controller = new AuthController(_authServiceMock.Object, _validatorMock.Object, _realCache);

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Headers.Origin = "https://app.taskforce.local";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };
        }

        public void Dispose()
        {
            _realCache.Dispose();
            GC.SuppressFinalize(this);
        }

        private static RegisterIdentityDto CreateValidDtoWithChallenge(byte[] challengeBytes)
        {
            string challengeBase64Url = WebEncoders.Base64UrlEncode(challengeBytes);
            string clientDataJson = $"{{\"challenge\":\"{challengeBase64Url}\"}}";
            byte[] clientDataBytes = Encoding.UTF8.GetBytes(clientDataJson);

            string clientDataJsonBase64Url = WebEncoders.Base64UrlEncode(clientDataBytes);

            string dummyBase64Url = WebEncoders.Base64UrlEncode(new byte[] { 1, 2, 3 });

            string attestationJson = $@"{{
        ""id"": ""{dummyBase64Url}"",
        ""rawId"": ""{dummyBase64Url}"",
        ""type"": ""public-key"",
        ""response"": {{
            ""clientDataJSON"": ""{clientDataJsonBase64Url}"",
            ""attestationObject"": ""{dummyBase64Url}""
        }}
        }}";

            return new RegisterIdentityDto(
                "U3VwZXJTZWNyZXRCbG9i", "5", "Dev",
                JsonDocument.Parse(attestationJson).RootElement
            );
        }
        private static CredentialCreateOptions CreateDummyOptions(byte[] challenge) => new()
        {
            Rp = new PublicKeyCredentialRpEntity("taskforce.local", "TaskForce", null),
            User = new Fido2User { Name = "test", DisplayName = "test", Id = new byte[] { 1 } },
            Challenge = challenge,
            PubKeyCredParams = new List<PubKeyCredParam>()
        };

        [Fact]
        public void GetRegisterOptions_Should_ReturnOk_And_SetCache()
        {
            var expectedChallenge = new byte[] { 1, 2, 3, 4, 5 };
            var fakeOptions = CreateDummyOptions(expectedChallenge);

            _authServiceMock.Setup(s => s.RequestNewCredential("taskforce.local"))
                            .Returns(fakeOptions);

            var result = _controller.GetRegisterOptions();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(fakeOptions);

            var expectedCacheKey = WebEncoders.Base64UrlEncode(expectedChallenge);
            _realCache.TryGetValue(expectedCacheKey, out CredentialCreateOptions? cachedOptions).Should().BeTrue();
            cachedOptions.Should().NotBeNull();
        }

        [Fact]
        public async Task Register_Should_ReturnBadRequest_When_ValidationFails()
        {
            var dto = CreateValidDtoWithChallenge(new byte[] { 1, 2, 3 });
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Title", "Erreur") });

            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

            var result = await _controller.Register(dto);

            result.Should().BeOfType<BadRequestObjectResult>();
            _authServiceMock.Verify(s => s.RegisterIdentityAsync(It.IsAny<RegisterIdentityDto>(), It.IsAny<CredentialCreateOptions>(), It.IsAny<AuthenticatorAttestationRawResponse>()), Times.Never);
        }

        [Fact]
        public async Task Register_Should_ReturnBadRequest_When_CacheMiss()
        {
            var challengeBytes = new byte[] { 9, 9, 9 };
            var dto = CreateValidDtoWithChallenge(challengeBytes);

            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            var result = await _controller.Register(dto);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().BeEquivalentTo(new { message = "Le défi a expiré ou est invalide. Veuillez recommencer l'inscription." });
        }

        [Fact]
        public async Task Register_Should_ReturnCreated_When_Success()
        {
            var challengeBytes = new byte[] { 10, 20, 30 };
            var dto = CreateValidDtoWithChallenge(challengeBytes);

            _validatorMock.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            var cacheKey = WebEncoders.Base64UrlEncode(challengeBytes);
            var originalOptions = CreateDummyOptions(challengeBytes);
            _realCache.Set(cacheKey, originalOptions);

            var dbIdentity = new DbIdentity
            {
                Id = Guid.NewGuid(),
                EncryptedProfile = Array.Empty<byte>(),
                Experience = "5",
                Title = "Dev"
            };

            _authServiceMock.Setup(s => s.RegisterIdentityAsync(dto, originalOptions, It.IsAny<AuthenticatorAttestationRawResponse>()))
                            .ReturnsAsync(dbIdentity);

            var result = await _controller.Register(dto);

            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(AuthController.Register));

            _realCache.TryGetValue(cacheKey, out _).Should().BeFalse();
        }
    }
}