using Domain.Enums;

namespace Application.DTOs.OrdemServicos;

public record RegistrarPagamentoRequest(
    MeioPagamento Meio,
    decimal Valor,
    DateTime DataPagamento,
    DateTime? ExpectedUpdatedAt = null
);
