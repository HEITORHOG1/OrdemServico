using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record OrdemServicoTaxaResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);
