namespace Domain.Exceptions;

public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }
}
