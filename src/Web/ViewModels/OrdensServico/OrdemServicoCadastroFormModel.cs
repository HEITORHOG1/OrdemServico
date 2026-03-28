namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoCadastroFormModel
{
    public string ClienteId { get; set; } = string.Empty;
    public string? EquipamentoId { get; set; }
    public string Defeito { get; set; } = string.Empty;
    public string? Duracao { get; set; }
    public string? Observacoes { get; set; }
    public string? Referencia { get; set; }
    public DateTime? ValidadeOrcamento { get; set; }
    public DateTime? PrazoEntrega { get; set; }
}
