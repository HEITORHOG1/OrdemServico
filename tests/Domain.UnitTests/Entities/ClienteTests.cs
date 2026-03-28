using Domain.Entities;

namespace Domain.UnitTests.Entities;

public class ClienteTests
{
    [Fact]
    public void CriarComNomeValidoDeveCriarCliente()
    {
        var cliente = Cliente.Criar("Cliente Teste", "123", "11999990000", "cliente@teste.com", "Rua A");

        Assert.NotEqual(Guid.Empty, cliente.Id);
        Assert.Equal("Cliente Teste", cliente.Nome);
    }

    [Fact]
    public void CriarComNomeVazioDeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Cliente.Criar(" ", null, null, null, null));
    }

    [Fact]
    public void AtualizarDeveAtualizarDados()
    {
        var cliente = Cliente.Criar("Cliente Teste", "123", "11999990000", "cliente@teste.com", "Rua A");

        cliente.Atualizar("Cliente Novo", "999", "11888880000", "novo@teste.com", "Rua B");

        Assert.Equal("Cliente Novo", cliente.Nome);
        Assert.Equal("999", cliente.Documento);
    }
}
