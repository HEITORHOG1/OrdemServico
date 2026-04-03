using System.Text.Json.Nodes;
using Web.Services.Api;

namespace Web.Services.Auth;

public sealed class AuthApi : IAuthApi
{
    private readonly IApiClient _apiClient;

    public AuthApi(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<JsonObject>> LoginAsync(JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync("/api/auth/login", request, cancellationToken);

    public Task<ApiResult<JsonObject>> RefreshTokenAsync(JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync("/api/auth/refresh", request, cancellationToken);

    public Task<ApiResult<JsonObject>> ObterUsuarioAtualAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetObjectAsync("/api/auth/me", cancellationToken);
}
