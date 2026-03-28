namespace Domain.Exceptions;

/// <summary>
/// Classe base para todas as exceções de domínio.
/// Não é sealed pois serve de base para as demais exceções específicas.
/// A camada de Apresentação (API) pode capturar essa exceção e convertê-la num HTTP 400 ou 422.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
