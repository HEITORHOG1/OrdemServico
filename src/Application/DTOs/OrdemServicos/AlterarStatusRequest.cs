using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record AlterarStatusRequest(
    StatusOS NovoStatus,
    DateTime? ExpectedUpdatedAt = null
);
