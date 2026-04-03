namespace Web.Models.Responses;

public sealed record UsuarioResponseModel(
    Guid Id,
    string Nome,
    string Email,
    string Cargo,
    Guid? TenantId,
    bool Ativo
);
