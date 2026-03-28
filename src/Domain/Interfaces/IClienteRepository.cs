using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repositório gerador das interações com a Entidade Cliente.
/// </summary>
public interface IClienteRepository
{
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cliente>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}
