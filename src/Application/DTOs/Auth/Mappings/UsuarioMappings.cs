using Domain.Entities;

namespace Application.DTOs.Auth.Mappings;

public static class UsuarioMappings
{
    public static UsuarioResponse ToResponse(this Usuario usuario)
    {
        return new UsuarioResponse(
            Id: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Cargo: usuario.Cargo.ToString(),
            TenantId: usuario.TenantId,
            Ativo: usuario.Ativo
        );
    }
}
