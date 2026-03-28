namespace Application.DTOs.Clientes;

public record CriarClienteRequest(
    string Nome,
    string? Documento,
    string? Telefone,
    string? Email,
    string? Endereco
);
