using Api.Back.DTOs.Requests;
using Api.Back.Validators;
using FluentValidation.TestHelper;
using System.Text.Json;
using Xunit;

namespace Api.Back.UnitTests.Validators
{
    public class RegisterIdentityValidatorTests
    {
        private readonly RegisterIdentityValidator _validator = new();

        private static RegisterIdentityDto CreateValidDto(
            string encryptedProfileBlob = "encrypted-blob",
            string experience = "5+",
            string title = "Développeur",
            JsonElement? webAuthnAttestationResponse = null)
        {
            return new RegisterIdentityDto(
                encryptedProfileBlob,
                "John",
                "Doe",
                experience,
                title,
                webAuthnAttestationResponse ?? JsonDocument.Parse("{}").RootElement
            );
        }

        [Fact]
        public void Should_NotHaveError_When_DtoIsValid()
        {
            // Arrange
            var dto = CreateValidDto();

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_HaveError_When_EncryptedProfileBlobIsEmptyOrNull(string? value)
        {
            // Arrange
            var dto = CreateValidDto(encryptedProfileBlob: value!);

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EncryptedProfileBlob);
        }

        [Fact]
        public void Should_HaveError_When_ExperienceExceedsMaxLength()
        {
            // Arrange
            var dto = CreateValidDto(experience: "abc");

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Experience);
        }

        [Fact]
        public void Should_NotHaveError_When_ExperienceIsAtMaxLength()
        {
            // Arrange
            var dto = CreateValidDto(experience: "10");

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Experience);
        }

        [Fact]
        public void Should_HaveError_When_TitleExceedsMaxLength()
        {
            // Arrange
            var dto = CreateValidDto(title: new string('a', 101));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_NotHaveError_When_TitleIsAtMaxLength()
        {
            // Arrange
            var dto = CreateValidDto(title: new string('a', 100));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Title);
        }

        [Fact]
        public void Should_HaveError_When_WebAuthnAttestationResponseIsUndefined()
        {
            // Arrange
            var dto = CreateValidDto(webAuthnAttestationResponse: default(JsonElement));

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.WebAuthnAttestationResponse);
        }
    }
}