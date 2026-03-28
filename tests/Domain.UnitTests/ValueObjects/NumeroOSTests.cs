using Domain.ValueObjects;

namespace Domain.UnitTests.ValueObjects;

public class NumeroOSTests
{
    [Fact]
    public void GerarDeveFormatarCorretamente()
    {
        var numero = NumeroOS.Gerar(new DateTime(2026, 3, 14), 42);

        Assert.Equal("OS-20260314-0042", numero.Valor);
    }

    [Fact]
    public void GerarComSequencialInvalidoDeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => NumeroOS.Gerar(DateTime.UtcNow, 0));
    }

    [Fact]
    public void ParseComFormatoInvalidoDeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => NumeroOS.Parse("OS-ERRADO"));
    }

    [Fact]
    public void ParseComFormatoValidoDeveRestaurarNumero()
    {
        var numero = NumeroOS.Parse("OS-20260314-0001");

        Assert.Equal("OS-20260314-0001", numero.Valor);
    }
}
