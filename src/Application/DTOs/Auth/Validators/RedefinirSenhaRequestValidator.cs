using FluentValidation;

namespace Application.DTOs.Auth.Validators;

public sealed class RedefinirSenhaRequestValidator : AbstractValidator<RedefinirSenhaRequest>
{
    public RedefinirSenhaRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email e obrigatorio.")
            .EmailAddress().WithMessage("Email invalido.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("O token de redefinicao e obrigatorio.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("A nova senha e obrigatoria.")
            .MinimumLength(8).WithMessage("A nova senha deve ter no minimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A nova senha deve conter ao menos uma letra maiuscula.")
            .Matches("[0-9]").WithMessage("A nova senha deve conter ao menos um numero.");

        RuleFor(x => x.ConfirmarNovaSenha)
            .Equal(x => x.NovaSenha).WithMessage("A confirmacao de senha nao confere.");
    }
}
