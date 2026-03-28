using Moq;
using System.Net;
using System.Text.Json.Nodes;
using Web.Services.Api;

namespace Web.UnitTests.Services.Api;

public sealed class EquipamentosApiTests
{
    [Fact]
    public async Task CriarAsyncDelegatesToApiClientWithExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var request = new JsonObject { ["tipo"] = "Notebook" };

        apiClient
            .Setup(x => x.PostAsync("/api/equipamentos", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.Created, new JsonObject(), null));

        var sut = new EquipamentosApi(apiClient.Object);
        var result = await sut.CriarAsync(request);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.PostAsync("/api/equipamentos", request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarPorClienteAsyncDelegatesToApiClientWithExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var clienteId = Guid.NewGuid();

        apiClient
            .Setup(x => x.GetArrayAsync($"/api/equipamentos/cliente/{clienteId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonArray>(true, HttpStatusCode.OK, new JsonArray(), null));

        var sut = new EquipamentosApi(apiClient.Object);
        var result = await sut.ListarPorClienteAsync(clienteId);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.GetArrayAsync($"/api/equipamentos/cliente/{clienteId}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
