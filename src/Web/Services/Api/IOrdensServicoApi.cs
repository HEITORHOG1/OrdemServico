using System.Text.Json.Nodes;

namespace Web.Services.Api;

public interface IOrdensServicoApi
{
    Task<ApiResult<JsonObject>> CriarAsync(JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult<JsonObject>> ListarAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ApiResult> AtualizarBaseAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AdicionarServicoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AdicionarProdutoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AplicarDescontoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AdicionarTaxaAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> RegistrarPagamentoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AdicionarAnotacaoAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
    Task<ApiResult> AlterarStatusAsync(Guid id, JsonObject request, CancellationToken cancellationToken = default);
}
