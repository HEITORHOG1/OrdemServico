namespace Application.DTOs.OrdemServicos;

public record CriarOrdemServicoRequest(
    Guid ClienteId,
    Guid? EquipamentoId,
    string Defeito,
    string? Duracao,
    string? Observacoes,
    string? Referencia,
    DateOnly? ValidadeOrcamento,
    DateOnly? PrazoEntrega
);
