namespace Application.DTOs.Clientes;

public record ClienteResponse(
    Guid Id,
    string Nome,
    string? Documento,
    string? Telefone,
    string? Email,
    string? Endereco,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
