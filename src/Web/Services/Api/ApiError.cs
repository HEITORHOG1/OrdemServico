namespace Web.Services.Api;

public sealed class ApiError
{
    public string Message { get; }
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    public ApiError(string message, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        Message = message;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }
}
