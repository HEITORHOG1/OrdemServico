using System.Text.Json.Nodes;

namespace Web.Services.Api;

public interface IClientesApi
{
    Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonArray>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}
