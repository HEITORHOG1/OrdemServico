using System.Text.Json.Nodes;

namespace Web.Services.Api;

public interface IApiClient
{
    Task<ApiResult<JsonObject>> GetObjectAsync(string path, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonArray>> GetArrayAsync(string path, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> PostAsync(string path, JsonObject payload, CancellationToken cancellationToken = default);
    Task<ApiResult> PostNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default);
    Task<ApiResult> PutNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default);
    Task<ApiResult> PatchNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default);
}
