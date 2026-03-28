using System.Text.Json.Nodes;

namespace Web.Services.Api;

public sealed class OrdensServicoApi : IOrdensServicoApi
{
    private readonly IApiClient _apiClient;

    public OrdensServicoApi(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync("/api/ordens-servico", request, cancellationToken);

    public Task<ApiResult<JsonObject>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _apiClient.GetObjectAsync($"/api/ordens-servico/{id}", cancellationToken);

    public Task<ApiResult<JsonObject>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => _apiClient.GetObjectAsync($"/api/ordens-servico?Page={page}&PageSize={pageSize}", cancellationToken);

    public Task<ApiResult> AtualizarBaseAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PutNoContentAsync($"/api/ordens-servico/{id}", request, cancellationToken);

    public Task<ApiResult> AdicionarServicoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/servicos", request, cancellationToken);

    public Task<ApiResult> AdicionarProdutoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/produtos", request, cancellationToken);

    public Task<ApiResult> AplicarDescontoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/desconto", request, cancellationToken);

    public Task<ApiResult> AdicionarTaxaAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/taxas", request, cancellationToken);

    public Task<ApiResult> RegistrarPagamentoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/pagamentos", request, cancellationToken);

    public Task<ApiResult> AdicionarAnotacaoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PostNoContentAsync($"/api/ordens-servico/{id}/anotacoes", request, cancellationToken);

    public Task<ApiResult> AlterarStatusAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default)
        => _apiClient.PatchNoContentAsync($"/api/ordens-servico/{id}/status", request, cancellationToken);
}
