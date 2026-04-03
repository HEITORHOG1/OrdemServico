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
