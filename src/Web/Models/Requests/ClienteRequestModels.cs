using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record CriarClienteRequestModel(
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("documento")] string? Documento,
    [property: JsonPropertyName("telefone")] string? Telefone,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("endereco")] string? Endereco);
