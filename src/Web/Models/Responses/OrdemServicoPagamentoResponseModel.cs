using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record OrdemServicoPagamentoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("meioPagamento")] MeioPagamentoModel MeioPagamento,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("dataPagamento")] DateTime DataPagamento);
