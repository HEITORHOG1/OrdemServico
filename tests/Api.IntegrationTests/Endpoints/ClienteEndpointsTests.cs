using System.Net;
using System.Net.Http.Json;
using Api.IntegrationTests.Fixtures;
using Api.IntegrationTests.TestData;
using Application.DTOs.Clientes;

namespace Api.IntegrationTests.Endpoints;

[Collection(ApiTestSuite.Name)]
public class ClienteEndpointsTests
{
    private readonly WebApplicationFixture _fixture;

    public ClienteEndpointsTests(WebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostEGetClienteDeveCriarERecuperarComSucesso()
    {
        await _fixture.ResetDatabaseAsync();
        using var client = _fixture.CreateApiClient();

        var request = OrdemServicoTestData.NovoCliente();

        var postResponse = await client.PostAsJsonAsync("/api/clientes/", request);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var cliente = await postResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.NotNull(cliente);

        var getResponse = await client.GetAsync($"/api/clientes/{cliente!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var clienteRecuperado = await getResponse.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.NotNull(clienteRecuperado);
        Assert.Equal(cliente.Id, clienteRecuperado!.Id);
    }
}
