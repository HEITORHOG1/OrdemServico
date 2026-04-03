using FluentValidation;

namespace Application.DTOs.Clientes.Validators;

public sealed class CriarClienteValidator : AbstractValidator<CriarClienteRequest>
{
    public CriarClienteValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .MaximumLength(150).WithMessage("O nome do cliente deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("E-mail com formato inválido.");
    }
}
