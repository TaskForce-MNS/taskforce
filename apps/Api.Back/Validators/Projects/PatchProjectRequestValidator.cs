using FluentValidation;
using Api.Back.DTOs.Requests.Projects;

namespace Api.Back.Validators.Projects
{
    public class PatchProjectRequestValidator : AbstractValidator<PatchProjectRequest>
    {
        public PatchProjectRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Le nom du projet ne peut pas dépasser 50 caractères.");

            RuleFor(x => x.ColorHex)
                .MaximumLength(7).WithMessage("Le code couleur ne peut pas dépasser 7 caractères.")
                .Matches("^#(?:[0-9a-fA-F]{3}){1,2}$")
                    .When(x => !string.IsNullOrEmpty(x.ColorHex), ApplyConditionTo.CurrentValidator)
                .WithMessage("Le code couleur doit être un format Hex valide (ex: #FFFFFF ou #FFF).");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(255).WithMessage("L'URL de l'image ne peut pas dépasser 255 caractères.");

        }
    }

}
