using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Principal repositório. Controle sob toda a representação orquestrada através do Aggregate Root.
/// </summary>
public interface IOrdemServicoRepository
{
    Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retorna a OS já montada com *todas* as coleções filhas necessárias para uso (serviços, produtos, etc).
    /// </summary>
    Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lista as Ordens de Serviço de forma paginada.
    /// </summary>
    Task<IEnumerable<OrdemServico>> ListarPaginadoAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);

    Task<int> ContarAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Fornece atomicamente o número sequencial numérico diário daquele dia, protegendo contra data-races, 
    /// para compor a string descritiva da Ordem de Serviço (OS-YYYYMMDD-[SEQ]).
    /// </summary>
    Task<int> ObterProximoSequencialNoDiaAsync(DateTime dataReferencia, CancellationToken cancellationToken = default);
}
