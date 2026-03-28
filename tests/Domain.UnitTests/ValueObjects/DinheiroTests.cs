using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class DinheiroTests
{
    [Fact]
    public void ConstrutorDeveArredondarParaDuasCasas()
    {
        var dinheiro = new Dinheiro(10.129m);

        Assert.Equal(10.13m, dinheiro.Valor);
    }

    [Fact]
    public void OperadorSomaDeveSomarValores()
    {
        var a = new Dinheiro(10m);
        var b = new Dinheiro(15m);

        var resultado = a + b;

        Assert.Equal(25m, resultado.Valor);
    }

    [Fact]
    public void OperadorSubtracaoQuandoResultadoNegativoDeveLancarArgumentException()
    {
        var a = new Dinheiro(10m);
        var b = new Dinheiro(15m);

        Assert.Throws<ArgumentException>(() => _ = a - b);
    }

    [Fact]
    public void ConstrutorComValorNegativoDeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Dinheiro(-1m));
    }
}
