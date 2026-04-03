using System.Text.Json.Nodes;
using Web.Services.Api;

namespace Web.Services.Auth;

public interface IAuthApi
{
    Task<ApiResult<JsonObject>> LoginAsync(JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> RefreshTokenAsync(JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> ObterUsuarioAtualAsync(CancellationToken cancellationToken = default);
}
