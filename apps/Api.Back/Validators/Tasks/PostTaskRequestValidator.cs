using FluentValidation;
using Api.Back.DTOs.Requests.Task;

namespace Api.Back.Validators.Tasks
{
    public class PostTaskRequestValidator : AbstractValidator<PostTaskRequest>
    {
        public PostTaskRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom de la tâche est requis.")
                .MaximumLength(255).WithMessage("Le nom ne peut pas dépasser 255 caractères.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("La description est trop longue.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("L'identifiant du projet est requis.");
        }

    }
}