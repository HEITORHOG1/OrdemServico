using Application.DTOs.OrdemServicos;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Moq;

namespace Application.UnitTests.Services;

public class OrdemServicoServiceTests
{
    [Fact]
    public async Task CriarAsyncQuandoClienteNaoExisteDeveLancarDomainException()
    {
        var osRepo = new Mock<IOrdemServicoRepository>();
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();
        clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var sut = new OrdemServicoService(osRepo.Object, clienteRepo.Object, uow.Object);
        var request = new CriarOrdemServicoRequest(Guid.NewGuid(), null, "Nao liga", "2h", null, null, null, null);

        await Assert.ThrowsAsync<DomainException>(() => sut.CriarAsync(request));
        osRepo.Verify(x => x.AdicionarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsyncComDadosValidosDevePersistirEDevolverResponse()
    {
        var osRepo = new Mock<IOrdemServicoRepository>();
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();

        clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Cliente.Criar("Cliente", null, null, null, null));
        osRepo.Setup(x => x.ObterProximoSequencialNoDiaAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new OrdemServicoService(osRepo.Object, clienteRepo.Object, uow.Object);
        var request = new CriarOrdemServicoRequest(Guid.NewGuid(), null, "Nao liga", "2h", null, null, null, null);

        var response = await sut.CriarAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.StartsWith("OS-", response.Numero);
        osRepo.Verify(x => x.AdicionarAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarServicoAsyncQuandoOsNaoExisteDeveLancarDomainException()
    {
        var osRepo = new Mock<IOrdemServicoRepository>();
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();
        osRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServico?)null);

        var sut = new OrdemServicoService(osRepo.Object, clienteRepo.Object, uow.Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            sut.AdicionarServicoAsync(Guid.NewGuid(), new AdicionarServicoRequest("Servico", 1, 10m)));

        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
