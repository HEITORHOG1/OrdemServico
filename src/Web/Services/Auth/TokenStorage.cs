namespace Web.Services.Auth;

/// <summary>
/// Armazena tokens JWT em memoria no circuito Blazor Server (scoped per-circuit).
/// Nao depende de ProtectedSessionStorage/DataProtection — evita problemas com key ring em containers.
/// Tokens existem enquanto o circuito SignalR estiver ativo (mesma aba do browser).
/// </summary>
public sealed class TokenStorage
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }

    public Task SalvarTokensAsync(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        return Task.CompletedTask;
    }

    public Task<(string? AccessToken, string? RefreshToken)> ObterTokensAsync()
        => Task.FromResult((AccessToken, RefreshToken));

    public Task LimparTokensAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        return Task.CompletedTask;
    }
}
