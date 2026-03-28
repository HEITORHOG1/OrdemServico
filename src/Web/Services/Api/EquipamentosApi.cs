using System.Text.Json.Nodes;

namespace Web.Services.Api;

public sealed class EquipamentosApi : IEquipamentosApi
{
    private readonly IApiClient _apiClient;

    public EquipamentosApi(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync("/api/equipamentos", request, cancellationToken);

    public Task<ApiResult<JsonArray>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
        => _apiClient.GetArrayAsync($"/api/equipamentos/cliente/{clienteId}", cancellationToken);
}
