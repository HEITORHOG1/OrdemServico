namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoComposicaoItemFormModel
{
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
}
