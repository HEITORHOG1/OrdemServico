using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public sealed class AdicionarTaxaValidator : AbstractValidator<AdicionarTaxaRequest>
{
    public AdicionarTaxaValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição da taxa é obrigatória.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("O valor da taxa deve ser maior que zero.");
    }
}
