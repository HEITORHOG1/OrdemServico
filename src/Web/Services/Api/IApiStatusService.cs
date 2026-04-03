namespace Web.Services.Api;

public interface IApiStatusService
{
    Task<ApiStatusResult> CheckAsync(CancellationToken cancellationToken = default);
}
