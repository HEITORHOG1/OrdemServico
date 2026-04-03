using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record AdicionarTaxaRequestModel(
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);
