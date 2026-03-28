namespace Domain.Interfaces;

/// <summary>
/// Contrato de Serviço de Cache distribuído para a camada de Application abstrair a Infrastructure (Redis).
/// </summary>
public interface ICacheService
{
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default);
    Task DefinirAsync<T>(string chave, T valor, TimeSpan? tempoExpiracao = null, CancellationToken cancellationToken = default);
    Task RemoverAsync(string chave, CancellationToken cancellationToken = default);
}
