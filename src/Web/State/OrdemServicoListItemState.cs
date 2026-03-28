using Web.Models;

namespace Web.State;

public sealed record OrdemServicoListItemState(
    Guid Id,
    string Numero,
    StatusOsModel Status,
    Guid ClienteId,
    string Defeito,
    decimal ValorTotal,
    DateTime CreatedAt);
