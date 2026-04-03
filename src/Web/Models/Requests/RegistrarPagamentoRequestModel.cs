using System.Text.Json.Serialization;

namespace Web.Models.Requests;

public sealed record RegistrarPagamentoRequestModel(
    [property: JsonPropertyName("meio")] MeioPagamentoModel Meio,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("dataPagamento")] DateTime DataPagamento,
    [property: JsonPropertyName("expectedUpdatedAt")] DateTime? ExpectedUpdatedAt = null);
