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

public sealed record OrdemServicoServicoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario,
    [property: JsonPropertyName("subtotal")] decimal Subtotal);

public sealed record OrdemServicoProdutoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario,
    [property: JsonPropertyName("subtotal")] decimal Subtotal);

public sealed record OrdemServicoTaxaResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);

public sealed record OrdemServicoPagamentoResponseModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("meioPagamento")] MeioPagamentoModel MeioPagamento,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("dataPagamento")] DateTime DataPagamento);
