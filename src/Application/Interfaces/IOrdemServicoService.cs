using Application.DTOs.Comuns;
using Application.DTOs.OrdemServicos;

namespace Application.Interfaces;

public interface IOrdemServicoService
{
    Task<OrdemServicoResponse> CriarAsync(CriarOrdemServicoRequest request, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Guid id, AtualizarOrdemServicoRequest request, CancellationToken cancellationToken = default);
    Task<OrdemServicoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrdemServicoResumoResponse>> ListarPaginadoAsync(PagedRequest request, CancellationToken cancellationToken = default);
    
    Task AdicionarServicoAsync(Guid id, AdicionarServicoRequest request, CancellationToken cancellationToken = default);
    Task AdicionarProdutoAsync(Guid id, AdicionarProdutoRequest request, CancellationToken cancellationToken = default);
    Task AplicarDescontoAsync(Guid id, AplicarDescontoRequest request, CancellationToken cancellationToken = default);
    Task AdicionarTaxaAsync(Guid id, AdicionarTaxaRequest request, CancellationToken cancellationToken = default);
    Task RegistrarPagamentoAsync(Guid id, RegistrarPagamentoRequest request, CancellationToken cancellationToken = default);
    Task AdicionarAnotacaoAsync(Guid id, AdicionarAnotacaoRequest request, CancellationToken cancellationToken = default);
    Task AlterarStatusAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default);
}
