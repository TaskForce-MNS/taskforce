using FluentValidation;
using Api.Back.DTOs.Requests.Task;

namespace Api.Back.Validators.Tasks
{
    public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom de la tâche ne peut pas être vide.")
                .MaximumLength(255).WithMessage("Le nom ne peut pas dépasser 255 caractères.")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("La description est trop longue.")
                .When(x => x.Description != null);

            RuleFor(x => x)
                .Must(x => !(x.UnassignTask && x.AssigneeId.HasValue))
                .WithMessage("Impossible de fournir un AssigneeId et demander une désassignation simultanément.")
                .WithName("AssigneeId");
        }
    }
}