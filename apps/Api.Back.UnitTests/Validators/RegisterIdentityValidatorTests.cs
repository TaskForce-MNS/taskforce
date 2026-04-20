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

        // 🛠️ Création du Record via son constructeur primaire
        private static RegisterIdentityDto CreateValidDto()
        {
            return new RegisterIdentityDto(
                "U3VwZXJTZWNyZXRCbG9i", // EncryptedProfileBlob
                "John",                 // FirstName
                "Doe",                  // LastName
                "5",                    // Experience
                "Développeur Fullstack",// Title
                JsonDocument.Parse("{\"id\":\"123456\"}").RootElement // WebAuthnAttestationResponse
            );
        }

        [Fact]
        public void Should_NotHaveAnyError_When_DtoIsPerfectlyValid()
        {
            var dto = CreateValidDto();
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        public void Should_HaveError_When_EncryptedProfileBlob_IsMissing(string invalidBlob)
        {
            var dto = CreateValidDto() with { EncryptedProfileBlob = invalidBlob };
            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EncryptedProfileBlob)
                  .WithErrorMessage("Le profil chiffré est obligatoire.");
        }

        [Theory]
        [InlineData("")]
        public void Should_HaveError_When_Experience_IsMissing(string invalidExperience)
        {
            var dto = CreateValidDto() with { Experience = invalidExperience };
            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Experience)
                  .WithErrorMessage("L'expérience est obligatoire.");
        }

        [Fact]
        public void Should_HaveError_When_Experience_ExceedsTwoCharacters()
        {
            var dto = CreateValidDto() with { Experience = "999" };
            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Experience)
                  .WithErrorMessage("L'expérience ne doit pas dépasser 2 caractères.");
        }

        [Theory]
        [InlineData("")]
        public void Should_HaveError_When_Title_IsMissing(string invalidTitle)
        {
            var dto = CreateValidDto() with { Title = invalidTitle };
            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Le titre est obligatoire.");
        }

        [Fact]
        public void Should_HaveError_When_Title_Exceeds100Characters()
        {
            var dto = CreateValidDto() with { Title = new string('A', 101) };
            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Le titre ne doit pas dépasser 100 caractères.");
        }

        [Fact]
        public void Should_HaveError_When_WebAuthnAttestationResponse_IsMissing()
        {
            // On crée un DTO avec un JsonElement non initialisé (état Undefined)
            var dto = CreateValidDto() with { WebAuthnAttestationResponse = default };

            // On utilise TestValidate (plus puissant pour les tests)
            var result = _validator.TestValidate(dto);

            // On vérifie l'erreur proprement
            result.ShouldHaveValidationErrorFor(x => x.WebAuthnAttestationResponse)
                  .WithErrorMessage("La réponse de la clé de sécurité (Passkey) est manquante.");
        }
    }
}