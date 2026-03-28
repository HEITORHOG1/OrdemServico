using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record EquipamentoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("clienteId")] Guid ClienteId,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("marca")] string? Marca,
    [property: JsonPropertyName("modelo")] string? Modelo,
    [property: JsonPropertyName("numeroSerie")] string? NumeroSerie,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt);
