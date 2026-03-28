using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record OrdemServicoResumoResponse(
    Guid Id,
    string Numero,
    StatusOS Status,
    Guid ClienteId,
    string Defeito,
    decimal ValorTotal,
    DateTime CreatedAt
);
