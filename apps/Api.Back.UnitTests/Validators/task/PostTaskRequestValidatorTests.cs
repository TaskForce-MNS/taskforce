using Api.Back.DTOs.Requests.Task;
using Api.Back.Models;
using Api.Back.Validators.Tasks;
using FluentValidation.TestHelper;
using Xunit;

namespace Api.Back.UnitTests.Validators.Task
{
    public class PostTaskRequestValidatorTests
    {
        private readonly PostTaskRequestValidator _sut = new();

        private static PostTaskRequest CreateValidRequest(
            Guid? projectId = null,
            string name = "Tâche valide",
            string? description = "Description ok") =>
            new(
                projectId ?? Guid.NewGuid(),
                name,
                description,
                null,
                null
            );

        [Fact]
        public void Should_HaveError_When_NameIsEmpty()
        {
            var request = CreateValidRequest(name: "");

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Le nom de la tâche est requis.");
        }

        [Fact]
        public void Should_HaveError_When_NameIsNull()
        {
            var request = CreateValidRequest(name: null!);

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_HaveError_When_NameExceedsMaxLength()
        {
            var request = CreateValidRequest(name: new string('a', 256));

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Le nom ne peut pas dépasser 255 caractères.");
        }

        [Fact]
        public void Should_NotHaveError_When_NameIsExactlyMaxLength()
        {
            var request = CreateValidRequest(name: new string('a', 255));

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_HaveError_When_DescriptionExceedsMaxLength()
        {
            var request = CreateValidRequest(description: new string('a', 2001));

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("La description est trop longue.");
        }

        [Fact]
        public void Should_NotHaveError_When_DescriptionIsNull()
        {
            var request = CreateValidRequest(description: null);

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_NotHaveError_When_DescriptionIsEmpty()
        {
            var request = CreateValidRequest(description: "");

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_HaveError_When_ProjectIdIsEmpty()
        {
            var request = CreateValidRequest(projectId: Guid.Empty);

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.ProjectId)
                .WithErrorMessage("L'identifiant du projet est requis.");
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_RequestIsFullyValid()
        {
            var request = CreateValidRequest();

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_OptionalFieldsAreNull()
        {
            var request = new PostTaskRequest(
                Guid.NewGuid(),
                "Tâche minimale",
                null,
                null,
                null,
                TaskDifficulty.None,
                null
            );

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}