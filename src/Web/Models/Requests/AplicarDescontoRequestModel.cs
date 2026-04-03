using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record AplicarDescontoRequestModel(
    [property: JsonPropertyName("tipo")] TipoDescontoModel Tipo,
    [property: JsonPropertyName("valor")] decimal Valor);
