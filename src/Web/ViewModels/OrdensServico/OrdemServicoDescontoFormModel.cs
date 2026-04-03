using Web.Models;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoDescontoFormModel
{
    public TipoDescontoModel Tipo { get; set; } = TipoDescontoModel.ValorFixo;
    public decimal Valor { get; set; }
}
