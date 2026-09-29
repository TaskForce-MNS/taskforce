using Api.Back.DTOs.Requests.Task;
using Api.Back.Validators.Tasks;
using FluentValidation.TestHelper;
using Xunit;

namespace Api.Back.UnitTests.Validators.Task
{
    public class UpdateTaskRequestValidatorTests
    {
        private readonly UpdateTaskRequestValidator _sut = new();

        private static UpdateTaskRequest CreateRequest(
            string? name = null,
            string? description = null) =>
            new(
                name,
                description,
                null,
                null,
                null,
                null,
                null,
                null
            );

        [Fact]
        public void Should_NotHaveError_When_NameIsNull()
        {
            var request = CreateRequest(name: null);

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_HaveError_When_NameIsProvided_But_Empty()
        {
            var request = CreateRequest(name: "");

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Le nom de la tâche ne peut pas être vide.");
        }

        [Fact]
        public void Should_HaveError_When_NameExceedsMaxLength()
        {
            var request = CreateRequest(name: new string('a', 256));

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage("Le nom ne peut pas dépasser 255 caractères.");
        }

        [Fact]
        public void Should_NotHaveError_When_NameIsValid()
        {
            var request = CreateRequest(name: "Nom modifié");

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_NotHaveError_When_DescriptionIsNull()
        {
            var request = CreateRequest(description: null);

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_HaveError_When_DescriptionExceedsMaxLength()
        {
            var request = CreateRequest(description: new string('a', 2001));

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("La description est trop longue.");
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_AllFieldsAreNull()
        {
            var request = CreateRequest();

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_NotHaveAnyErrors_When_AllFieldsAreValid()
        {
            var request = CreateRequest(name: "Nouveau nom", description: "Nouvelle description");

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }
        [Fact]
        public void Should_HaveError_When_UnassignTaskIsTrue_And_AssigneeIdIsProvided()
        {
            var request = new UpdateTaskRequest(
                null, null, null, null,
                Guid.NewGuid(),
                null, null, null,
                UnassignTask: true
            );

            var result = _sut.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.AssigneeId)
                .WithErrorMessage("Impossible de fournir un AssigneeId et demander une désassignation simultanément.");
        }

        [Fact]
        public void Should_NotHaveError_When_UnassignTaskIsTrue_And_AssigneeIdIsNull()
        {
            var request = new UpdateTaskRequest(
                null, null, null, null,
                null,
                null, null, null,
                UnassignTask: true
            );

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.AssigneeId);
        }

        [Fact]
        public void Should_NotHaveError_When_UnassignTaskIsFalse_And_AssigneeIdIsProvided()
        {
            var request = new UpdateTaskRequest(
                null, null, null, null,
                Guid.NewGuid(),
                null, null, null,
                UnassignTask: false
            );

            var result = _sut.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.AssigneeId);
        }
    }

}