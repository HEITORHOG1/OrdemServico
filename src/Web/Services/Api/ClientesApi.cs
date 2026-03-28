using System.Text.Json.Nodes;

namespace Web.Services.Api;

public sealed class ClientesApi : IClientesApi
{
    private readonly IApiClient _apiClient;

    public ClientesApi(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync("/api/clientes", request, cancellationToken);

    public Task<ApiResult<JsonObject>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.GetObjectAsync($"/api/clientes/{id}", cancellationToken);

    public Task<ApiResult<JsonArray>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
        => _apiClient.GetArrayAsync($"/api/clientes/busca?nome={Uri.EscapeDataString(nome)}", cancellationToken);
}
