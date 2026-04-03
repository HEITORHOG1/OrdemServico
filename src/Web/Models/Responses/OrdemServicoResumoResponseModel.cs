using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record OrdemServicoResumoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("numero")] string Numero,
    [property: JsonPropertyName("status")] StatusOsModel Status,
    [property: JsonPropertyName("clienteId")] Guid ClienteId,
    [property: JsonPropertyName("defeito")] string Defeito,
    [property: JsonPropertyName("valorTotal")] decimal ValorTotal,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt);
