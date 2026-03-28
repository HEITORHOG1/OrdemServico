using FluentValidation;

namespace Application.DTOs.Auth.Validators;

public sealed class RegistrarUsuarioRequestValidator : AbstractValidator<RegistrarUsuarioRequest>
{
    public RegistrarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome e obrigatorio.")
            .MaximumLength(150).WithMessage("O nome deve ter no maximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email e obrigatorio.")
            .EmailAddress().WithMessage("Email invalido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha e obrigatoria.")
            .MinimumLength(8).WithMessage("A senha deve ter no minimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiuscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um numero.");

        RuleFor(x => x.ConfirmarSenha)
            .Equal(x => x.Senha).WithMessage("A confirmacao de senha nao confere.");

        RuleFor(x => x.Cargo)
            .IsInEnum().WithMessage("Cargo invalido.");
    }
}
