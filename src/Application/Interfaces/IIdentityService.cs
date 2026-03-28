namespace Application.Interfaces;

/// <summary>
/// Facade sobre o ASP.NET Identity (UserManager).
/// Implementado na Infrastructure para nao acoplar Application ao Identity.
/// </summary>
public interface IIdentityService
{
    Task<(bool Sucesso, string IdentityUserId, IEnumerable<string> Erros)> CriarUsuarioAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default);

    Task<bool> ValidarCredenciaisAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default);

    Task<string?> ObterIdentityUserIdPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> AlterarSenhaAsync(
        string identityUserId,
        string senhaAtual,
        string novaSenha,
        CancellationToken cancellationToken = default);

    Task<string> GerarTokenRedefinicaoSenhaAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> RedefinirSenhaAsync(
        string email,
        string token,
        string novaSenha,
        CancellationToken cancellationToken = default);

    Task AdicionarAoCargoAsync(
        string identityUserId,
        string cargo,
        CancellationToken cancellationToken = default);
}
