using Moq;
using System.Net;
using System.Text.Json.Nodes;
using Web.Services.Api;

namespace Web.UnitTests.Services.Api;

public sealed class OrdensServicoApiTests
{
    [Fact]
    public async Task ListarAsyncDelegatesToExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();

        apiClient
            .Setup(x => x.GetObjectAsync("/api/ordens-servico?Page=2&PageSize=25", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.OK, new JsonObject(), null));

        var sut = new OrdensServicoApi(apiClient.Object);
        var result = await sut.ListarAsync(2, 25);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.GetObjectAsync("/api/ordens-servico?Page=2&PageSize=25", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdicionarServicoAsyncDelegatesToExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var osId = Guid.NewGuid();
        var payload = new JsonObject { ["descricao"] = "Diagnostico" };

        apiClient
            .Setup(x => x.PostNoContentAsync($"/api/ordens-servico/{osId}/servicos", payload, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult.Success(HttpStatusCode.NoContent));

        var sut = new OrdensServicoApi(apiClient.Object);
        var result = await sut.AdicionarServicoAsync(osId, payload);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.PostNoContentAsync($"/api/ordens-servico/{osId}/servicos", payload, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarStatusAsyncDelegatesToExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var osId = Guid.NewGuid();
        var payload = new JsonObject { ["novoStatus"] = 1 };

        apiClient
            .Setup(x => x.PatchNoContentAsync($"/api/ordens-servico/{osId}/status", payload, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApiResult.Success(HttpStatusCode.NoContent));

        var sut = new OrdensServicoApi(apiClient.Object);
        var result = await sut.AlterarStatusAsync(osId, payload);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.PatchNoContentAsync($"/api/ordens-servico/{osId}/status", payload, It.IsAny<CancellationToken>()), Times.Once);
    }
}
