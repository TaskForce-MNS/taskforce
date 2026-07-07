using Api.Back.DTOs.Requests.Projects;
using Api.Back.Validators.Projects;
using FluentValidation.TestHelper;
using Xunit;

namespace Api.Back.UnitTests.Validators.Projects
{
    public class PatchProjectRequestValidatorTests
    {
        private readonly PatchProjectRequestValidator _validator = new();

        [Fact]
        public void Should_NotHaveError_When_AllFieldsAreDefaultNull()
        {
            var request = new PatchProjectRequest();

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveError_When_AllFieldsAreValid()
        {
            var request = new PatchProjectRequest(
                Name: "Mon Projet",
                Description: "Une description",
                ColorHex: "#FFF",
                ImageUrl: "https://example.com/image.png"
            );

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ---- Name ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_NotHaveError_When_NameIsNullOrEmpty(string? value)
        {
            var request = new PatchProjectRequest(Name: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_HaveError_When_NameExceedsMaxLength()
        {
            var request = new PatchProjectRequest(Name: new string('a', 51));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_NotHaveError_When_NameIsAtMaxLength()
        {
            var request = new PatchProjectRequest(Name: new string('a', 50));

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        // ---- ColorHex ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_NotHaveError_When_ColorHexIsNullOrEmpty(string? value)
        {
            var request = new PatchProjectRequest(ColorHex: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ColorHex);
        }

        [Theory]
        [InlineData("#FFF")]
        [InlineData("#FFFFFF")]
        [InlineData("#a1b2c3")]
        public void Should_NotHaveError_When_ColorHexIsValidFormat(string value)
        {
            var request = new PatchProjectRequest(ColorHex: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ColorHex);
        }

        [Theory]
        [InlineData("blue")]
        [InlineData("123456")]
        [InlineData("#GGG")]
        [InlineData("#FFFFFFF")]
        public void Should_HaveError_When_ColorHexIsInvalidFormat(string value)
        {
            var request = new PatchProjectRequest(ColorHex: value);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ColorHex);
        }

        // ---- ImageUrl ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_NotHaveError_When_ImageUrlIsNullOrEmpty(string? value)
        {
            var request = new PatchProjectRequest(ImageUrl: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
        }

        [Fact]
        public void Should_HaveError_When_ImageUrlExceedsMaxLength()
        {
            var request = new PatchProjectRequest(ImageUrl: new string('a', 256));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
        }

        [Fact]
        public void Should_NotHaveError_When_ImageUrlIsAtMaxLength()
        {
            var request = new PatchProjectRequest(ImageUrl: new string('a', 255));

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
        }
    }
}