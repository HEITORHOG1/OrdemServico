using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.UnitTests.Entities;

public class OrdemServicoTests
{
    [Fact]
    public void CriarDeveIniciarEmRascunho()
    {
        var os = CriarOsValida();

        Assert.Equal(StatusOS.Rascunho, os.Status);
    }

    [Fact]
    public void AprovarAPartirDeRascunhoDevePermitirTransicao()
    {
        var os = CriarOsValida();

        os.Aprovar();

        Assert.Equal(StatusOS.Aprovada, os.Status);
    }

    [Fact]
    public void ConcluirTrabalhoAPartirDeRascunhoDeveLancarStatusTransicaoInvalidaException()
    {
        var os = CriarOsValida();

        Assert.Throws<StatusTransicaoInvalidaException>(() => os.ConcluirTrabalho());
    }

    [Fact]
    public void AtualizarDadosBasicosComStatusAprovadaDeveLancarDomainException()
    {
        var os = CriarOsValida();
        os.Aprovar();

        Assert.Throws<DomainException>(() =>
            os.AtualizarDadosBasicos("Novo defeito", "1h", "Obs", "Ref", null, null));
    }

    [Fact]
    public void FinalizarEEntregarSemPagamentoTotalDeveLancarDomainException()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Troca de tela", 1, 100m);
        os.Aprovar();
        os.IniciarAndamento();
        os.ConcluirTrabalho();

        Assert.Throws<DomainException>(() => os.FinalizarEEntregar());
    }

    [Fact]
    public void FinalizarEEntregarComPagamentoTotalDeveEntregar()
    {
        var os = CriarOsValida();
        os.AdicionarServico("Troca de tela", 1, 100m);
        os.Aprovar();
        os.IniciarAndamento();
        os.ConcluirTrabalho();
        os.AdicionarPagamento(MeioPagamento.PIX, 100m, DateTime.UtcNow);

        os.FinalizarEEntregar();

        Assert.Equal(StatusOS.Entregue, os.Status);
    }

    private static OrdemServico CriarOsValida()
        => OrdemServico.Criar(Guid.NewGuid(), null, "Nao liga", "2h", null, null, null, null);
}
