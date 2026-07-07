using Api.Back.DTOs.Requests.Projects;
using Api.Back.Validators.Projects;
using FluentValidation.TestHelper;
using Xunit;

namespace Api.Back.UnitTests.Validators.Projects
{
    public class PostProjectRequestValidatorTests
    {
        private readonly PostProjectRequestValidator _validator = new();

        private static PostProjectRequest CreateValidRequest(
            string name = "Mon Projet",
            string? description = "Une description",
            string? colorHex = "#FFF",
            string? imageUrl = "https://example.com/image.png")
        {
            return new PostProjectRequest(name, description, colorHex, imageUrl);
        }

        [Fact]
        public void Should_NotHaveError_When_RequestIsValid()
        {
            var request = CreateValidRequest();

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveError_When_OptionalFieldsAreNull()
        {
            var request = CreateValidRequest(colorHex: null, imageUrl: null);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ---- Name ----

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_HaveError_When_NameIsEmptyOrNull(string? value)
        {
            var request = CreateValidRequest(name: value!);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_HaveError_When_NameExceedsMaxLength()
        {
            var request = CreateValidRequest(name: new string('a', 51));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_NotHaveError_When_NameIsAtMaxLength()
        {
            var request = CreateValidRequest(name: new string('a', 50));

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        // ---- ColorHex ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_NotHaveError_When_ColorHexIsNullOrEmpty(string? value)
        {
            var request = CreateValidRequest(colorHex: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ColorHex);
        }

        [Theory]
        [InlineData("#FFF")]
        [InlineData("#FFFFFF")]
        [InlineData("#a1b2c3")]
        public void Should_NotHaveError_When_ColorHexIsValidFormat(string value)
        {
            var request = CreateValidRequest(colorHex: value);

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
            var request = CreateValidRequest(colorHex: value);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ColorHex);
        }

        // ---- ImageUrl ----

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Should_NotHaveError_When_ImageUrlIsNullOrEmpty(string? value)
        {
            var request = CreateValidRequest(imageUrl: value);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
        }

        [Fact]
        public void Should_HaveError_When_ImageUrlExceedsMaxLength()
        {
            var request = CreateValidRequest(imageUrl: new string('a', 256));

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
        }

        [Fact]
        public void Should_NotHaveError_When_ImageUrlIsAtMaxLength()
        {
            var request = CreateValidRequest(imageUrl: new string('a', 255));

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
        }
    }
}