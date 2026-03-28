namespace Web.Models;

public sealed record PaginationModel(int Page = 1, int PageSize = 10);

public sealed record OrdemServicoFilterModel(
    string? Numero,
    StatusOsModel? Status,
    Guid? ClienteId,
    int Page = 1,
    int PageSize = 10);

public sealed record SelectOptionModel<TValue>(TValue Value, string Label);
