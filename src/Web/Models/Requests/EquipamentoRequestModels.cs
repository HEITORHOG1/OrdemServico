using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record CriarEquipamentoRequestModel(
    [property: JsonPropertyName("clienteId")] Guid ClienteId,
    [property: JsonPropertyName("tipo")] string Tipo,
    [property: JsonPropertyName("marca")] string? Marca,
    [property: JsonPropertyName("modelo")] string? Modelo,
    [property: JsonPropertyName("numeroSerie")] string? NumeroSerie);
