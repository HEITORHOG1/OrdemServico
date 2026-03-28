using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public class CriarOrdemServicoValidator : AbstractValidator<CriarOrdemServicoRequest>
{
    public CriarOrdemServicoValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("O ClienteId é obrigatório.");

        RuleFor(x => x.Defeito)
            .NotEmpty().WithMessage("A descrição do defeito é obrigatória.");
    }
}
