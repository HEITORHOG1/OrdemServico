using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public class AtualizarOrdemServicoValidator : AbstractValidator<AtualizarOrdemServicoRequest>
{
    public AtualizarOrdemServicoValidator()
    {
        RuleFor(x => x.Defeito)
            .NotEmpty().WithMessage("A descrição do defeito não pode ser vazia ao atualizar.");
    }
}
