using Domain.Entities;

namespace Application.DTOs.Clientes.Mappings;

public static class ClienteMappings
{
    public static ClienteResponse ToResponse(this Cliente cliente)
    {
        return new ClienteResponse(
            Id: cliente.Id,
            Nome: cliente.Nome,
            Documento: cliente.Documento,
            Telefone: cliente.Telefone,
            Email: cliente.Email,
            Endereco: cliente.Endereco,
            CreatedAt: cliente.CreatedAt,
            UpdatedAt: cliente.UpdatedAt
        );
    }
}
