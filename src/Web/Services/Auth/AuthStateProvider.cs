using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Services.Auth;

public sealed class AuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TokenStorage _tokenStorage;

    public AuthStateProvider(TokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var (accessToken, _) = await _tokenStorage.ObterTokensAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
            return AnonymousState;

        var claims = ParseClaimsFromJwt(accessToken);
        if (claims is null)
            return AnonymousState;

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotificarLogin()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public async Task NotificarLogoutAsync()
    {
        await _tokenStorage.LimparTokensAsync();
        NotifyAuthenticationStateChanged(
            Task.FromResult(AnonymousState));
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string jwt)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            if (token.ValidTo < DateTime.UtcNow)
                return null;

            return token.Claims;
        }
        catch
        {
            return null;
        }
    }
}
