using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record CriarOrdemServicoRequestModel(
    [property: JsonPropertyName("clienteId")] Guid ClienteId,
    [property: JsonPropertyName("equipamentoId")] Guid? EquipamentoId,
    [property: JsonPropertyName("defeito")] string Defeito,
    [property: JsonPropertyName("duracao")] string? Duracao,
    [property: JsonPropertyName("observacoes")] string? Observacoes,
    [property: JsonPropertyName("referencia")] string? Referencia,
    [property: JsonPropertyName("validadeOrcamento")] DateOnly? ValidadeOrcamento,
    [property: JsonPropertyName("prazoEntrega")] DateOnly? PrazoEntrega);
