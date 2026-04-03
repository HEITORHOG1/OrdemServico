using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record OrdemServicoProdutoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario,
    [property: JsonPropertyName("subtotal")] decimal Subtotal);
