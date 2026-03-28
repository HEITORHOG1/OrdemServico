using System.Net;

namespace Web.Services.Api;

public sealed class ApiResult
{
    public bool Succeeded { get; }
    public HttpStatusCode StatusCode { get; }
    public ApiError? Error { get; }

    private ApiResult(bool succeeded, HttpStatusCode statusCode, ApiError? error)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        Error = error;
    }

    public static ApiResult Success(HttpStatusCode statusCode)
        => new(true, statusCode, null);

    public static ApiResult Failure(HttpStatusCode statusCode, ApiError error)
        => new(false, statusCode, error);
}

public sealed class ApiResult<T>
{
    public bool Succeeded { get; }
    public HttpStatusCode StatusCode { get; }
    public T? Data { get; }
    public ApiError? Error { get; }

    public ApiResult(bool succeeded, HttpStatusCode statusCode, T? data, ApiError? error)
    {
        Succeeded = succeeded;
        StatusCode = statusCode;
        Data = data;
        Error = error;
    }
}

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
