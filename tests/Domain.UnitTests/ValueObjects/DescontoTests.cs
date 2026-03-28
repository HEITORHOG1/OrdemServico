using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class DescontoTests
{
    [Fact]
    public void CalcularValorEfetivoComPercentualDeveCalcularCorretamente()
    {
        var desconto = new Desconto(TipoDesconto.Percentual, 10m);

        var valor = desconto.CalcularValorEfetivo(200m);

        Assert.Equal(20m, valor);
    }

    [Fact]
    public void CalcularValorEfetivoComValorFixoDeveRetornarValorFixo()
    {
        var desconto = new Desconto(TipoDesconto.ValorFixo, 25m);

        var valor = desconto.CalcularValorEfetivo(200m);

        Assert.Equal(25m, valor);
    }

    [Fact]
    public void ConstrutorComPercentualMaiorQue100DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Desconto(TipoDesconto.Percentual, 120m));
    }

    [Fact]
    public void CalcularValorEfetivoQuandoDescontoExcedeSubtotalDeveLancarDescontoExcedeTotalException()
    {
        var desconto = new Desconto(TipoDesconto.ValorFixo, 250m);

        Assert.Throws<DescontoExcedeTotalException>(() => desconto.CalcularValorEfetivo(200m));
    }
}
