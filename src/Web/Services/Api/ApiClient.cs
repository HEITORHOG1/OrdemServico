using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace Web.Services.Api;

public sealed class ApiClient : IApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string ApiKeyHeaderName = "X-Api-Key";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IApiErrorParser _apiErrorParser;
    private readonly ApiSettings _apiSettings;

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        IApiErrorParser apiErrorParser,
        IOptions<ApiSettings> apiSettings)
    {
        _httpClientFactory = httpClientFactory;
        _apiErrorParser = apiErrorParser;
        _apiSettings = apiSettings.Value;
    }

    public async Task<ApiResult<JsonObject>> GetObjectAsync(string path, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return new ApiResult<JsonObject>(false, transportError.statusCode, null, transportError.error);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
                return new ApiResult<JsonObject>(false, response.StatusCode, null, error);
            }

            var data = await ReadJsonObjectAsync(response.Content, cancellationToken);
            return new ApiResult<JsonObject>(true, response.StatusCode, data, null);
        }
    }

    public async Task<ApiResult<JsonArray>> GetArrayAsync(string path, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return new ApiResult<JsonArray>(false, transportError.statusCode, null, transportError.error);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
                return new ApiResult<JsonArray>(false, response.StatusCode, null, error);
            }

            var data = await ReadJsonArrayAsync(response.Content, cancellationToken);
            return new ApiResult<JsonArray>(true, response.StatusCode, data, null);
        }
    }

    public async Task<ApiResult<JsonObject>> PostAsync(string path, JsonObject payload, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(
                () => CreateRequestWithJson(HttpMethod.Post, path, payload),
                cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return new ApiResult<JsonObject>(false, transportError.statusCode, null, transportError.error);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
                return new ApiResult<JsonObject>(false, response.StatusCode, null, error);
            }

            var data = await ReadJsonObjectAsync(response.Content, cancellationToken);
            return new ApiResult<JsonObject>(true, response.StatusCode, data, null);
        }
    }

    public async Task<ApiResult> PostNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(
                () => CreateRequestWithJson(HttpMethod.Post, path, payload),
                cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return ApiResult.Failure(transportError.statusCode, transportError.error);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success(response.StatusCode);
            }

            var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
            return ApiResult.Failure(response.StatusCode, error);
        }
    }

    public async Task<ApiResult> PutNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(
                () => CreateRequestWithJson(HttpMethod.Put, path, payload),
                cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return ApiResult.Failure(transportError.statusCode, transportError.error);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success(response.StatusCode);
            }

            var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
            return ApiResult.Failure(response.StatusCode, error);
        }
    }

    public async Task<ApiResult> PatchNoContentAsync(string path, JsonObject payload, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await SendWithRetryAsync(
                () => CreateRequestWithJson(HttpMethod.Patch, path, payload),
                cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            var transportError = CreateTransportError(ex);
            return ApiResult.Failure(transportError.statusCode, transportError.error);
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Success(response.StatusCode);
            }

            var error = await _apiErrorParser.ParseAsync(response.StatusCode, response.Content, cancellationToken);
            return ApiResult.Failure(response.StatusCode, error);
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var attempt = 0;

        while (true)
        {
            attempt++;
            try
            {
                var client = _httpClientFactory.CreateClient("OsApi");
                using var request = requestFactory();
                PrepareRequestHeaders(request);
                var response = await client.SendAsync(request, cancellationToken);

                if ((int)response.StatusCode >= 500 && attempt < maxRetries)
                {
                    response.Dispose();
                    await Task.Delay(GetRetryDelay(attempt), cancellationToken);
                    continue;
                }

                return response;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                if (cancellationToken.IsCancellationRequested || attempt >= maxRetries)
                {
                    throw;
                }

                await Task.Delay(GetRetryDelay(attempt), cancellationToken);
            }
        }
    }

    private static TimeSpan GetRetryDelay(int attempt)
        => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1));

    private static HttpRequestMessage CreateRequestWithJson(HttpMethod method, string path, JsonObject payload)
    {
        var request = new HttpRequestMessage(method, path);
        var json = payload.ToJsonString(JsonOptions);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    private static async Task<JsonObject> ReadJsonObjectAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var jsonNode = await JsonNode.ParseAsync(await content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return jsonNode as JsonObject ?? new JsonObject();
    }

    private static async Task<JsonArray> ReadJsonArrayAsync(HttpContent content, CancellationToken cancellationToken)
    {
        var jsonNode = await JsonNode.ParseAsync(await content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return jsonNode as JsonArray ?? new JsonArray();
    }

    private static (HttpStatusCode statusCode, ApiError error) CreateTransportError(Exception ex)
    {
        if (ex is TaskCanceledException)
        {
            return (HttpStatusCode.RequestTimeout, new ApiError("A API demorou para responder. Verifique sua conexao e tente novamente."));
        }

        return (HttpStatusCode.ServiceUnavailable, new ApiError("Nao foi possivel conectar na API. Verifique se o servico esta ativo e tente novamente."));
    }

    private void PrepareRequestHeaders(HttpRequestMessage request)
    {
        if (!request.Headers.Contains(CorrelationIdHeaderName))
        {
            request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, Guid.NewGuid().ToString("N"));
        }

        if (!string.IsNullOrWhiteSpace(_apiSettings.ApiKey) && !request.Headers.Contains(ApiKeyHeaderName))
        {
            request.Headers.TryAddWithoutValidation(ApiKeyHeaderName, _apiSettings.ApiKey);
        }
    }
}
