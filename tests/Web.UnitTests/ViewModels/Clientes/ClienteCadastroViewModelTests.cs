using Moq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Clientes;

namespace Web.UnitTests.ViewModels.Clientes;

public sealed class ClienteCadastroViewModelTests
{
    [Fact]
    public async Task SubmitAsyncWhenNomeIsMissingReturnsFailureWithValidationError()
    {
        var clientesApi = new Mock<IClientesApi>(MockBehavior.Strict);
        var flowState = new ClienteFlowState();
        var viewModel = new ClienteCadastroViewModel(clientesApi.Object, flowState);

        viewModel.AbrirNovoCliente();
        viewModel.Form.Nome = string.Empty;

        var result = await viewModel.SubmitAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Corrija os campos obrigatorios antes de enviar.", result.Message);
        Assert.True(viewModel.ValidationErrors.ContainsKey("Nome"));
        Assert.False(viewModel.IsSubmitting);
        clientesApi.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SubmitAsyncWhenApiReturnsSuccessSetsFlowStateAndResetsForm()
    {
        var clientesApi = new Mock<IClientesApi>();
        var flowState = new ClienteFlowState();
        var viewModel = new ClienteCadastroViewModel(clientesApi.Object, flowState);

        var clienteId = Guid.NewGuid();
        var response = new ClienteResponseModel(
            clienteId,
            "Cliente Teste",
            "12345678900",
            "11999998888",
            "cliente@teste.local",
            "Rua Teste",
            DateTime.UtcNow,
            DateTime.UtcNow);

        var responseJson = JsonSerializer.SerializeToNode(response) as JsonObject ?? new JsonObject();
        clientesApi
            .Setup(api => api.CriarAsync(It.IsAny<JsonObject>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResult<JsonObject>(true, HttpStatusCode.Created, responseJson, null));

        viewModel.AbrirNovoCliente();
        viewModel.Form.Nome = "Cliente Teste";
        viewModel.Form.Documento = "12345678900";

        var result = await viewModel.SubmitAsync();

        Assert.True(result.Succeeded);
        Assert.Equal("Cliente cadastrado com sucesso.", result.Message);
        Assert.False(viewModel.FormVisivel);
        Assert.Equal(clienteId, flowState.UltimoClienteId);
        Assert.Single(viewModel.ClientesCriados);
        Assert.Equal(clienteId, viewModel.ClientesCriados[0].Id);

        clientesApi.Verify(api => api.CriarAsync(It.IsAny<JsonObject>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
