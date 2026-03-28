using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public sealed class RegistrarPagamentoValidator : AbstractValidator<RegistrarPagamentoRequest>
{
    public RegistrarPagamentoValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor do pagamento deve ser maior que zero.");

        RuleFor(x => x.Meio)
            .IsInEnum().WithMessage("Meio de pagamento inválido.");

        RuleFor(x => x.DataPagamento)
            .NotEmpty().WithMessage("A data do pagamento é obrigatória.");
    }
}
