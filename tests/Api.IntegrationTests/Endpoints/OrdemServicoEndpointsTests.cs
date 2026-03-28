using System.Net;
using System.Net.Http.Json;
using Api.IntegrationTests.Fixtures;
using Api.IntegrationTests.TestData;
using Application.DTOs.Clientes;
using Application.DTOs.OrdemServicos;

namespace Api.IntegrationTests.Endpoints;

[Collection(ApiTestSuite.Name)]
public class OrdemServicoEndpointsTests
{
    private readonly WebApplicationFixture _fixture;

    public OrdemServicoEndpointsTests(WebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FluxoCompletoDeveCriarOSEConsultar()
    {
        await _fixture.ResetDatabaseAsync();
        using var client = _fixture.CreateApiClient();

        var clienteRequest = OrdemServicoTestData.NovoCliente();
        var clienteResponse = await client.PostAsJsonAsync("/api/clientes/", clienteRequest);
        Assert.Equal(HttpStatusCode.Created, clienteResponse.StatusCode);
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.NotNull(cliente);

        var osRequest = OrdemServicoTestData.NovaOrdemServico(cliente!.Id);
        var osCreateResponse = await client.PostAsJsonAsync("/api/ordens-servico/", osRequest);
        Assert.Equal(HttpStatusCode.Created, osCreateResponse.StatusCode);
        var os = await osCreateResponse.Content.ReadFromJsonAsync<OrdemServicoResponse>();
        Assert.NotNull(os);

        var getResponse = await client.GetAsync($"/api/ordens-servico/{os!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var osAtualizada = await getResponse.Content.ReadFromJsonAsync<OrdemServicoResponse>();
        Assert.NotNull(osAtualizada);
        Assert.NotEmpty(osAtualizada!.Numero);
        Assert.Equal(cliente.Id, osAtualizada.ClienteId);

        var listResponse = await client.GetAsync("/api/ordens-servico?page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
    }
}
