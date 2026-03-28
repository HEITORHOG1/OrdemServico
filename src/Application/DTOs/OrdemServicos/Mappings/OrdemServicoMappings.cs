using Domain.Entities;

namespace Application.DTOs.OrdemServicos.Mappings;

public static class OrdemServicoMappings
{
    public static OrdemServicoResponse ToResponse(this OrdemServico os)
    {
        return new OrdemServicoResponse(
            Id: os.Id,
            Numero: os.Numero?.Valor ?? string.Empty,
            Status: os.Status,
            ClienteId: os.ClienteId,
            EquipamentoId: os.EquipamentoId,
            Defeito: os.Defeito,
            LaudoTecnico: os.LaudoTecnico,
            Observacoes: os.Observacoes,
            CondicoesPagamento: os.CondicoesPagamento,
            Referencia: os.Referencia,
            Duracao: os.Duracao,
            ValidadeOrcamento: os.ValidadeOrcamento,
            PrazoEntrega: os.PrazoEntrega,
            ValorDesconto: os.DescontoAplicado?.Valor ?? 0,
            ValorTotal: os.CalcularTotalReal().Valor,
            CreatedAt: os.CreatedAt,
            UpdatedAt: os.UpdatedAt,
            Servicos: os.Servicos.Select(s => new OrdemServicoServicoResponse(s.Id, s.Descricao, s.Quantidade, s.ValorUnitario.Valor, s.Subtotal)).ToList().AsReadOnly(),
            Produtos: os.Produtos.Select(p => new OrdemServicoProdutoResponse(p.Id, p.Descricao, p.Quantidade, p.ValorUnitario.Valor, p.Subtotal)).ToList().AsReadOnly(),
            Taxas: os.Taxas.Select(t => new OrdemServicoTaxaResponse(t.Id, t.Descricao, t.Valor.Valor)).ToList().AsReadOnly(),
            Pagamentos: os.Pagamentos.Select(p => new OrdemServicoPagamentoResponse(p.Id, p.MeioPagamento, p.Valor.Valor, p.DataPagamento)).ToList().AsReadOnly()
        );
    }

    public static OrdemServicoResumoResponse ToResumoResponse(this OrdemServico os)
    {
        return new OrdemServicoResumoResponse(
            Id: os.Id,
            Numero: os.Numero?.Valor ?? string.Empty,
            Status: os.Status,
            ClienteId: os.ClienteId,
            Defeito: os.Defeito,
            ValorTotal: os.CalcularTotalReal().Valor,
            CreatedAt: os.CreatedAt
        );
    }
}
