using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record OrdemServicoPagamentoResponse(Guid Id, MeioPagamento MeioPagamento, decimal Valor, DateTime DataPagamento);
