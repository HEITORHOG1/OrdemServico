namespace Web.State;

public sealed class OperationResult
{
    public bool Succeeded { get; }
    public string? Message { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    private OperationResult(bool succeeded, string? message, IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Succeeded = succeeded;
        Message = message;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static OperationResult Success(string? message = null)
        => new(true, message, null);

    public static OperationResult Failure(string message, IReadOnlyDictionary<string, string[]>? validationErrors = null)
        => new(false, message, validationErrors);
}
