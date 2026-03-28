using System.Net;

namespace Web.Services.Api;

public interface IApiErrorParser
{
    Task<ApiError> ParseAsync(HttpStatusCode statusCode, HttpContent? content, CancellationToken cancellationToken = default);
}
