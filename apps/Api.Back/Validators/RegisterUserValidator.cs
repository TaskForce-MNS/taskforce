using Api.Back.DTOs.Requests;
using FluentValidation;

namespace Api.Back.Validators
{
    public class RegisterUserDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public RegisterUserDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("L'email est obligatoire.")
                .EmailAddress().WithMessage("Format email invalide.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
                .MinimumLength(12).WithMessage("Le mot de passe doit contenir au moins 12 caractères.")
                .Matches(@"[A-Z]").WithMessage("Le mot de passe doit contenir au moins une lettre majuscule.")
                .Matches(@"[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.");


            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty();
        }
    }
}