using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public sealed class AdicionarProdutoValidator : AbstractValidator<AdicionarProdutoRequest>
{
    public AdicionarProdutoValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descrição do produto é obrigatória.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");

        RuleFor(x => x.ValorUnitario)
            .GreaterThanOrEqualTo(0).WithMessage("O valor unitário não pode ser negativo.");
    }
}
