using Web.Models;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoPagamentoFormModel
{
    public MeioPagamentoModel Meio { get; set; } = MeioPagamentoModel.Pix;
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.Now;
}
