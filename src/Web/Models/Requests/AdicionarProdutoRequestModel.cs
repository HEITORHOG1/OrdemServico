using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record AdicionarProdutoRequestModel(
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario);
