using FluentValidation;

namespace Application.DTOs.OrdemServicos.Validators;

public sealed class AlterarStatusValidator : AbstractValidator<AlterarStatusRequest>
{
    public AlterarStatusValidator()
    {
        RuleFor(x => x.NovoStatus)
            .IsInEnum().WithMessage("Status informado é inválido.");
    }
}
