namespace Domain.Interfaces;

/// <summary>
/// Contrato para o padrão Unit Of Work.
/// Permite o controle transacional explícito na camada de Aplicação,
/// sem acoplar regras de negócio diretamente ao ORM (EF Core).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persiste todas as mudanças rastreadas através de uma transação.
    /// Retorna verdadeiro (true) se obteve sucesso.
    /// </summary>
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}
