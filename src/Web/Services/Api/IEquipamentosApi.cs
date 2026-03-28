using System.Text.Json.Nodes;

namespace Web.Services.Api;

public interface IEquipamentosApi
{
    Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonArray>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
