using System.Text.Json.Serialization;

namespace Web.Models.Responses;

public sealed record OrdemServicoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("numero")] string Numero,
    [property: JsonPropertyName("status")] StatusOsModel Status,
    [property: JsonPropertyName("clienteId")] Guid ClienteId,
    [property: JsonPropertyName("equipamentoId")] Guid? EquipamentoId,
    [property: JsonPropertyName("defeito")] string Defeito,
    [property: JsonPropertyName("laudoTecnico")] string? LaudoTecnico,
    [property: JsonPropertyName("observacoes")] string? Observacoes,
    [property: JsonPropertyName("condicoesPagamento")] string? CondicoesPagamento,
    [property: JsonPropertyName("referencia")] string? Referencia,
    [property: JsonPropertyName("duracao")] string? Duracao,
    [property: JsonPropertyName("validadeOrcamento")] DateOnly? ValidadeOrcamento,
    [property: JsonPropertyName("prazoEntrega")] DateOnly? PrazoEntrega,
    [property: JsonPropertyName("valorDesconto")] decimal ValorDesconto,
    [property: JsonPropertyName("valorTotal")] decimal ValorTotal,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt,
    [property: JsonPropertyName("updatedAt")] DateTime UpdatedAt,
    [property: JsonPropertyName("servicos")] IReadOnlyCollection<OrdemServicoServicoResponseModel> Servicos,
    [property: JsonPropertyName("produtos")] IReadOnlyCollection<OrdemServicoProdutoResponseModel> Produtos,
    [property: JsonPropertyName("taxas")] IReadOnlyCollection<OrdemServicoTaxaResponseModel> Taxas,
    [property: JsonPropertyName("pagamentos")] IReadOnlyCollection<OrdemServicoPagamentoResponseModel> Pagamentos);
