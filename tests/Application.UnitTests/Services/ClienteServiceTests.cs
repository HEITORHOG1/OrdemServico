using Application.DTOs.Clientes;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Moq;

namespace Application.UnitTests.Services;

public class ClienteServiceTests
{
    [Fact]
    public async Task CriarAsyncQuandoDocumentoJaExisteDeveLancarDomainException()
    {
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();

        clienteRepo.Setup(x => x.ExistePorDocumentoAsync("123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new ClienteService(clienteRepo.Object, uow.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<ClienteService>.Instance);
        var request = new CriarClienteRequest("Cliente", "123", null, null, null);

        await Assert.ThrowsAsync<DomainException>(() => sut.CriarAsync(request));
        clienteRepo.Verify(x => x.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsyncComDadosValidosDevePersistirEDevolverResponse()
    {
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new ClienteService(clienteRepo.Object, uow.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<ClienteService>.Instance);
        var request = new CriarClienteRequest("Cliente", "123", null, null, null);

        var response = await sut.CriarAsync(request);

        Assert.Equal("Cliente", response.Nome);
        clienteRepo.Verify(x => x.AdicionarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsyncQuandoNaoExisteDeveRetornarNull()
    {
        var clienteRepo = new Mock<IClienteRepository>();
        var uow = new Mock<IUnitOfWork>();
        clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var sut = new ClienteService(clienteRepo.Object, uow.Object, Microsoft.Extensions.Logging.Abstractions.NullLogger<ClienteService>.Instance);

        var response = await sut.ObterPorIdAsync(Guid.NewGuid());

        Assert.Null(response);
    }
}
