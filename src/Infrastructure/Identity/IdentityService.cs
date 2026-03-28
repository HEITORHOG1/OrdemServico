using Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppIdentityUser> _userManager;

    public IdentityService(UserManager<AppIdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Sucesso, string IdentityUserId, IEnumerable<string> Erros)> CriarUsuarioAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        var user = new AppIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, senha);

        return result.Succeeded
            ? (true, user.Id, Enumerable.Empty<string>())
            : (false, string.Empty, result.Errors.Select(e => e.Description));
    }

    public async Task<bool> ValidarCredenciaisAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return false;

        return await _userManager.CheckPasswordAsync(user, senha);
    }

    public async Task<string?> ObterIdentityUserIdPorEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<bool> AlterarSenhaAsync(
        string identityUserId,
        string senhaAtual,
        string novaSenha,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(identityUserId);
        if (user is null) return false;

        var result = await _userManager.ChangePasswordAsync(user, senhaAtual, novaSenha);
        return result.Succeeded;
    }

    public async Task<string> GerarTokenRedefinicaoSenhaAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new KeyNotFoundException("Usuario nao encontrado.");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> RedefinirSenhaAsync(
        string email,
        string token,
        string novaSenha,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return false;

        var result = await _userManager.ResetPasswordAsync(user, token, novaSenha);
        return result.Succeeded;
    }

    public async Task AdicionarAoCargoAsync(
        string identityUserId,
        string cargo,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(identityUserId)
            ?? throw new KeyNotFoundException("IdentityUser nao encontrado.");

        await _userManager.AddToRoleAsync(user, cargo);
    }
}
