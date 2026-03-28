using FluentValidation;

namespace Application.DTOs.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email e obrigatorio.")
            .EmailAddress().WithMessage("Email invalido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha e obrigatoria.");
    }
}
