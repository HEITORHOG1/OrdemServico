using Domain.Enums;
using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public sealed class AplicarDescontoValidator : AbstractValidator<AplicarDescontoRequest>
{
    public AplicarDescontoValidator()
    {
        RuleFor(x => x.Valor)
            .GreaterThanOrEqualTo(0).WithMessage("O valor do desconto não pode ser negativo.");

        RuleFor(x => x.Valor)
            .LessThanOrEqualTo(100).When(x => x.Tipo == TipoDesconto.Percentual)
            .WithMessage("O desconto percentual não pode ser maior que 100%.");
    }
}
