using Moq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models;
using Web.Models.Responses;
using Web.Services.Api;
using Web.ViewModels.OrdensServico;

namespace Web.UnitTests.ViewModels.OrdensServico;

public sealed class OrdemServicoListaViewModelTests
{
    [Fact]
    public async Task CarregarAsyncWhenApiSuccessPopulatesItemsAndPaging()
    {
        var ordensApi = new Mock<IOrdensServicoApi>();
        var viewModel = new OrdemServicoListaViewModel(ordensApi.Object);

        var pageModel = BuildPage(
            BuildResumo("OS-100", StatusOsModel.Rascunho),
            BuildResumo("OS-101", StatusOsModel.Orcamento));

        var pageJson = JsonSerializer.SerializeToNode(pageModel) as JsonObject ?? new JsonObject();

        ordensApi
            .Setup(api => api.ListarAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.OK, pageJson, null));

        var result = await viewModel.CarregarAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(2, viewModel.Itens.Count);
        Assert.Equal(2, viewModel.TotalCount);
        Assert.Equal(1, viewModel.Page);
        Assert.Equal(10, viewModel.PageSize);
        Assert.Equal(1, viewModel.TotalPages);
    }

    [Fact]
    public async Task ItensFiltradosWhenFiltersAreSetAppliesNumeroStatusAndCliente()
    {
        var ordensApi = new Mock<IOrdensServicoApi>();
        var viewModel = new OrdemServicoListaViewModel(ordensApi.Object);

        var clienteAlvo = Guid.NewGuid();
        var pageModel = BuildPage(
            BuildResumo("OS-200", StatusOsModel.Orcamento, clienteAlvo),
            BuildResumo("OS-201", StatusOsModel.Rascunho, Guid.NewGuid()));

        var pageJson = JsonSerializer.SerializeToNode(pageModel) as JsonObject ?? new JsonObject();

        ordensApi
            .Setup(api => api.ListarAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.OK, pageJson, null));

        var loadResult = await viewModel.CarregarAsync();
        Assert.True(loadResult.Succeeded);

        viewModel.NumeroFiltro = "200";
        viewModel.StatusFiltro = StatusOsModel.Orcamento;
        viewModel.ClienteIdFiltro = clienteAlvo.ToString("N")[..8];

        var filtrados = viewModel.ItensFiltrados;

        Assert.Single(filtrados);
        Assert.Equal("OS-200", filtrados[0].Numero);
    }

    private static OrdemServicoResumoResponseModel BuildResumo(string numero, StatusOsModel status, Guid? clienteId = null)
        => new(
            Guid.NewGuid(),
            numero,
            status,
            clienteId ?? Guid.NewGuid(),
            "Defeito teste",
            100m,
            DateTime.UtcNow);

    private static PagedResponseModel<OrdemServicoResumoResponseModel> BuildPage(params OrdemServicoResumoResponseModel[] items)
        => new(
            items,
            items.Length,
            1,
            10,
            1);
}
