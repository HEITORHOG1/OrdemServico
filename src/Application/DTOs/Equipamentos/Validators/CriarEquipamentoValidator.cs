using FluentValidation;

namespace Application.DTOs.Equipamentos.Validators;

public sealed class CriarEquipamentoValidator : AbstractValidator<CriarEquipamentoRequest>
{
    public CriarEquipamentoValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("O ClienteId é obrigatório.");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("O tipo do equipamento é obrigatório.");
    }
}
