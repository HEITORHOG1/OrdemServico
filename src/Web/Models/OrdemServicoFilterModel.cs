namespace Web.Models;

public sealed record OrdemServicoFilterModel(
    string? Numero,
    StatusOsModel? Status,
    Guid? ClienteId,
    int Page = 1,
    int PageSize = 10);
