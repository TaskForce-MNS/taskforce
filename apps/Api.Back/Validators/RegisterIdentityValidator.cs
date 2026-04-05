using Api.Back.DTOs.Requests;
using FluentValidation;
using System.Text.Json;

namespace Api.Back.Validators
{
    public class RegisterIdentityValidator : AbstractValidator<RegisterIdentityDto>
    {
        public RegisterIdentityValidator()
        {
            RuleFor(x => x.EncryptedProfileBlob)
                .NotEmpty().WithMessage("Le profil chiffré est obligatoire.");

            RuleFor(x => x.Experience)
                .NotEmpty().WithMessage("L'expérience est obligatoire.")
                .MaximumLength(2).WithMessage("L'expérience ne doit pas dépasser 2 caractères.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Le titre est obligatoire.")
                .MaximumLength(100).WithMessage("Le titre ne doit pas dépasser 100 caractères.");

            RuleFor(x => x.WebAuthnAttestationResponse)
                .Must(json => json.ValueKind != JsonValueKind.Undefined)
                .WithMessage("La réponse de la clé de sécurité (Passkey) est manquante.");
        }
    }

}
