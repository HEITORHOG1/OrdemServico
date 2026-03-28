using FluentValidation;

namespace Application.DTOs.Auth.Validators;

public sealed class AlterarSenhaRequestValidator : AbstractValidator<AlterarSenhaRequest>
{
    public AlterarSenhaRequestValidator()
    {
        RuleFor(x => x.SenhaAtual)
            .NotEmpty().WithMessage("A senha atual e obrigatoria.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("A nova senha e obrigatoria.")
            .MinimumLength(8).WithMessage("A nova senha deve ter no minimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A nova senha deve conter ao menos uma letra maiuscula.")
            .Matches("[0-9]").WithMessage("A nova senha deve conter ao menos um numero.");

        RuleFor(x => x.ConfirmarNovaSenha)
            .Equal(x => x.NovaSenha).WithMessage("A confirmacao de senha nao confere.");
    }
}
