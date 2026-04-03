using System.Net;
using System.Net.Http.Headers;

namespace Web.Services.Auth;

public sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly TokenStorage _tokenStorage;

    public AuthDelegatingHandler(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var (accessToken, _) = await _tokenStorage.ObterTokensAsync();

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Se receber 401, limpa tokens (sessao expirada)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokenStorage.LimparTokensAsync();
        }

        return response;
    }
}
