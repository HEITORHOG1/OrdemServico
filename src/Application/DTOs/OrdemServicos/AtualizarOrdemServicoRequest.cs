namespace Application.DTOs.OrdemServicos;

public record AtualizarOrdemServicoRequest(
    string Defeito,
    string? Duracao,
    string? Observacoes,
    string? Referencia,
    DateOnly? ValidadeOrcamento,
    DateOnly? PrazoEntrega
);
