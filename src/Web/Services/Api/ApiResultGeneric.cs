using System.Net;

namespace Web.Services.Api;

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
