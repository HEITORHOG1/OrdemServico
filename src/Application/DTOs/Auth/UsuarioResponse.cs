namespace Application.DTOs.Auth;

public sealed record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    string Cargo,
    Guid? TenantId,
    bool Ativo
);
