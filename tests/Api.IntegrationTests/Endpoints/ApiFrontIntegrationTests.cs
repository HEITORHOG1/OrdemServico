using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Api.IntegrationTests.Fixtures;
using Web.Models;
using Web.Models.Responses;
using Web.Services.Api;

namespace Api.IntegrationTests.Endpoints;

[Collection(ApiTestSuite.Name)]
public sealed class ApiFrontIntegrationTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly WebApplicationFixture _fixture;

    public ApiFrontIntegrationTests(WebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FluxoPrincipalApiFrontDeveExecutarComServicosWebContraApiReal()
    {
        await _fixture.ResetDatabaseAsync();

        using var httpClient = _fixture.CreateApiClient();
        var httpClientFactory = new StaticHttpClientFactory(httpClient);
        var apiSettings = Microsoft.Extensions.Options.Options.Create(new ApiSettings
        {
            BaseUrl = "https://localhost",
            ApiKey = string.Empty
        });

        var apiClient = new ApiClient(httpClientFactory, new ApiErrorParser(), apiSettings);
        var clientesApi = new ClientesApi(apiClient);
        var equipamentosApi = new EquipamentosApi(apiClient);
        var ordensApi = new OrdensServicoApi(apiClient);

        var clienteRequest = new JsonObject
        {
            ["nome"] = "Cliente Integrado",
            ["documento"] = Guid.NewGuid().ToString("N")[..11],
            ["telefone"] = "11988887777",
            ["email"] = "integrado@os.local",
            ["endereco"] = "Rua Integracao"
        };

        var clienteCreate = await clientesApi.CriarAsync(clienteRequest);
        Assert.True(clienteCreate.Succeeded);
        Assert.NotNull(clienteCreate.Data);

        var clienteId = clienteCreate.Data!["id"]!.GetValue<Guid>();
        Assert.NotEqual(Guid.Empty, clienteId);

        var equipamentoRequest = new JsonObject
        {
            ["clienteId"] = clienteId,
            ["tipo"] = "Notebook",
            ["marca"] = "Lenovo",
            ["modelo"] = "ThinkPad",
            ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
        };

        var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest);
        Assert.True(equipamentoCreate.Succeeded);
        var equipamentoId = equipamentoCreate.Data!["id"]!.GetValue<Guid>();

        var osCreateRequest = new JsonObject
        {
            ["clienteId"] = clienteId,
            ["equipamentoId"] = equipamentoId,
            ["defeito"] = "Nao liga",
            ["observacoes"] = "Fluxo integrado API+Front"
        };

        var osCreate = await ordensApi.CriarAsync(osCreateRequest);
        Assert.True(osCreate.Succeeded);

        var osId = osCreate.Data!["id"]!.GetValue<Guid>();
        var osGet = await ordensApi.ObterPorIdAsync(osId);
        Assert.True(osGet.Succeeded);

        var detalhe = osGet.Data!.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        Assert.NotNull(detalhe);

        var addServico = await ordensApi.AdicionarServicoAsync(osId, new JsonObject
        {
            ["descricao"] = "Diagnostico",
            ["quantidade"] = 1,
            ["valorUnitario"] = 150m
        });

        Assert.True(addServico.Succeeded);

        var osAfterServico = await ordensApi.ObterPorIdAsync(osId);
        Assert.True(osAfterServico.Succeeded);

        var detalheAtualizado = osAfterServico.Data!.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        Assert.NotNull(detalheAtualizado);

        var alterarStatus = await ordensApi.AlterarStatusAsync(osId, new JsonObject
        {
            ["novoStatus"] = (int)StatusOsModel.Orcamento,
            ["expectedUpdatedAt"] = detalheAtualizado!.UpdatedAt
        });

        Assert.True(alterarStatus.Succeeded);

        var osFinal = await ordensApi.ObterPorIdAsync(osId);
        Assert.True(osFinal.Succeeded);

        var finalDetalhe = osFinal.Data!.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        Assert.NotNull(finalDetalhe);
        Assert.Equal(StatusOsModel.Orcamento, finalDetalhe!.Status);
    }

    private sealed class StaticHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StaticHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }
}
