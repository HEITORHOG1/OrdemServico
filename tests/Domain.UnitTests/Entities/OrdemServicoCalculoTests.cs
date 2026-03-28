using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.UnitTests.Entities;

public class OrdemServicoCalculoTests
{
    [Fact]
    public void CalcularTotalRealSemDescontoDeveSomarServicosProdutosETaxas()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Mao de obra", 2, 50m);
        os.AdicionarProduto("Peca", 1, 30m);
        os.AdicionarTaxa("Frete", 20m);

        var total = os.CalcularTotalReal();

        Assert.Equal(150m, total.Valor);
    }

    [Fact]
    public void CalcularTotalRealComDescontoPercentualDeveAplicarPercentualNoSubtotal()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Mao de obra", 1, 100m);
        os.AdicionarProduto("Peca", 1, 100m);
        os.AdicionarTaxa("Frete", 10m);
        os.AplicarDesconto(new Desconto(TipoDesconto.Percentual, 10m));

        var total = os.CalcularTotalReal();

        Assert.Equal(190m, total.Valor);
    }

    [Fact]
    public void CalcularTotalRealComDescontoValorFixoDeveSubtrairValorFixo()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Mao de obra", 1, 120m);
        os.AdicionarTaxa("Frete", 10m);
        os.AplicarDesconto(new Desconto(TipoDesconto.ValorFixo, 20m));

        var total = os.CalcularTotalReal();

        Assert.Equal(110m, total.Valor);
    }

    [Fact]
    public void AplicarDescontoQueExcedeSubtotalDeveLancarDescontoExcedeTotalException()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Mao de obra", 1, 50m);

        Assert.Throws<DescontoExcedeTotalException>(() =>
            os.AplicarDesconto(new Desconto(TipoDesconto.ValorFixo, 60m)));
    }

    private static OrdemServico CriarOsValida()
        => OrdemServico.Criar(Guid.NewGuid(), null, "Nao liga", "2h", null, null, null, null);
}
