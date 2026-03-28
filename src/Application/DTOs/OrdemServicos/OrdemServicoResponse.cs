using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record OrdemServicoResponse(
    Guid Id,
    string Numero,
    StatusOS Status,
    Guid ClienteId,
    Guid? EquipamentoId,
    string Defeito,
    string? LaudoTecnico,
    string? Observacoes,
    string? CondicoesPagamento,
    string? Referencia,
    string? Duracao,
    DateOnly? ValidadeOrcamento,
    DateOnly? PrazoEntrega,
    decimal ValorDesconto,
    decimal ValorTotal,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<OrdemServicoServicoResponse> Servicos,
    IReadOnlyCollection<OrdemServicoProdutoResponse> Produtos,
    IReadOnlyCollection<OrdemServicoTaxaResponse> Taxas,
    IReadOnlyCollection<OrdemServicoPagamentoResponse> Pagamentos
);

public record OrdemServicoServicoResponse(Guid Id, string Descricao, int Quantidade, decimal ValorUnitario, decimal Subtotal);
public record OrdemServicoProdutoResponse(Guid Id, string Descricao, int Quantidade, decimal ValorUnitario, decimal Subtotal);
public record OrdemServicoTaxaResponse(Guid Id, string Descricao, decimal Valor);
public record OrdemServicoPagamentoResponse(Guid Id, MeioPagamento MeioPagamento, decimal Valor, DateTime DataPagamento);
