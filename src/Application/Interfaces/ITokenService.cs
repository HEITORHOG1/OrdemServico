namespace Application.Interfaces;

/// <summary>
/// Abstrai a geracao e validacao de JWT e RefreshToken.
/// Implementado na Infrastructure para nao acoplar Application a libs de JWT.
/// </summary>
public interface ITokenService
{
    Task<(string AccessToken, string RefreshToken, DateTime ExpiraEm)> GerarTokensAsync(
        string identityUserId,
        string email,
        string cargo,
        Guid usuarioId,
        Guid? tenantId,
        CancellationToken cancellationToken = default);

    Task<(string IdentityUserId, string Email)?> ValidarAccessTokenExpiradoAsync(
        string accessToken,
        CancellationToken cancellationToken = default);

    Task<bool> ValidarRefreshTokenAsync(
        string identityUserId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevogarRefreshTokenAsync(
        string identityUserId,
        string refreshToken,
        CancellationToken cancellationToken = default);
}
