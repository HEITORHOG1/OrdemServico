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

public sealed record AtualizarOrdemServicoRequestModel(
    [property: JsonPropertyName("defeito")] string Defeito,
    [property: JsonPropertyName("duracao")] string? Duracao,
    [property: JsonPropertyName("observacoes")] string? Observacoes,
    [property: JsonPropertyName("referencia")] string? Referencia,
    [property: JsonPropertyName("validadeOrcamento")] DateOnly? ValidadeOrcamento,
    [property: JsonPropertyName("prazoEntrega")] DateOnly? PrazoEntrega);

public sealed record AdicionarServicoRequestModel(
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario);

public sealed record AdicionarProdutoRequestModel(
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("quantidade")] int Quantidade,
    [property: JsonPropertyName("valorUnitario")] decimal ValorUnitario);

public sealed record AplicarDescontoRequestModel(
    [property: JsonPropertyName("tipo")] TipoDescontoModel Tipo,
    [property: JsonPropertyName("valor")] decimal Valor);

public sealed record AdicionarTaxaRequestModel(
    [property: JsonPropertyName("descricao")] string Descricao,
    [property: JsonPropertyName("valor")] decimal Valor);

public sealed record RegistrarPagamentoRequestModel(
    [property: JsonPropertyName("meio")] MeioPagamentoModel Meio,
    [property: JsonPropertyName("valor")] decimal Valor,
    [property: JsonPropertyName("dataPagamento")] DateTime DataPagamento,
    [property: JsonPropertyName("expectedUpdatedAt")] DateTime? ExpectedUpdatedAt = null);

public sealed record AdicionarAnotacaoRequestModel(
    [property: JsonPropertyName("texto")] string Texto,
    [property: JsonPropertyName("autor")] string Autor);

public sealed record AlterarStatusRequestModel(
    [property: JsonPropertyName("novoStatus")] StatusOsModel NovoStatus,
    [property: JsonPropertyName("expectedUpdatedAt")] DateTime? ExpectedUpdatedAt = null);
