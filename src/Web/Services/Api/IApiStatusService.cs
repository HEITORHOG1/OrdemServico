namespace Web.Services.Api;

public interface IApiStatusService
{
    Task<ApiStatusResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record ApiStatusResult(bool IsAvailable, string Message);
