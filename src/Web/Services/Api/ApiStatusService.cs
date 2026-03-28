using System.Net.Http.Json;

namespace Web.Services.Api;

public sealed class ApiStatusService : IApiStatusService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiStatusService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiStatusResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("OsApi");
            using var response = await client.GetAsync("/swagger/v1/swagger.json", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiStatusResult(false, $"API respondeu com status {(int)response.StatusCode}.");
            }

            var swagger = await response.Content.ReadFromJsonAsync<object>(cancellationToken: cancellationToken);
            return swagger is not null
                ? new ApiStatusResult(true, "API acessivel e contrato Swagger carregado.")
                : new ApiStatusResult(false, "API respondeu, mas o payload Swagger veio vazio.");
        }
        catch (Exception ex)
        {
            return new ApiStatusResult(false, $"Falha ao acessar API: {ex.Message}");
        }
    }
}
