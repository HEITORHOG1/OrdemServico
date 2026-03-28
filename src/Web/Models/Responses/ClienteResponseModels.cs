using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record ClienteResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("documento")] string? Documento,
    [property: JsonPropertyName("telefone")] string? Telefone,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("endereco")] string? Endereco,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt);
