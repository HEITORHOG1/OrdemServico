using Web.Models;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoComposicaoItemFormModel
{
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
}

public sealed class OrdemServicoTaxaFormModel
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public sealed class OrdemServicoDescontoFormModel
{
    public TipoDescontoModel Tipo { get; set; } = TipoDescontoModel.ValorFixo;
    public decimal Valor { get; set; }
}

public sealed class OrdemServicoAnotacaoFormModel
{
    public string Texto { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
}

public sealed class OrdemServicoPagamentoFormModel
{
    public MeioPagamentoModel Meio { get; set; } = MeioPagamentoModel.Pix;
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.Now;
}
