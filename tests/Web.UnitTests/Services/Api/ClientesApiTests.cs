using Moq;
using System.Net;
using System.Text.Json.Nodes;
using Web.Services.Api;

namespace Web.UnitTests.Services.Api;

public sealed class ClientesApiTests
{
    [Fact]
    public async Task CriarAsyncDelegatesToApiClientWithExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var request = new JsonObject { ["nome"] = "Cliente Mock" };

        apiClient
            .Setup(x => x.PostAsync("/api/clientes", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.Created, new JsonObject(), null));

        var sut = new ClientesApi(apiClient.Object);
        var result = await sut.CriarAsync(request);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.PostAsync("/api/clientes", request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsyncDelegatesToApiClientWithExpectedPath()
    {
        var apiClient = new Mock<IApiClient>();
        var id = Guid.NewGuid();

        apiClient
            .Setup(x => x.GetObjectAsync($"/api/clientes/{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.OK, new JsonObject(), null));

        var sut = new ClientesApi(apiClient.Object);
        var result = await sut.ObterPorIdAsync(id);

        Assert.True(result.Succeeded);
        apiClient.Verify(x => x.GetObjectAsync($"/api/clientes/{id}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
