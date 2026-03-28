using Application.DTOs.Auth;
using Application.DTOs.Auth.Mappings;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IIdentityService _identityService;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IIdentityService identityService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _identityService = identityService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var credenciaisValidas = await _identityService.ValidarCredenciaisAsync(request.Email, request.Senha, cancellationToken);
        if (!credenciaisValidas)
            throw new DomainException("Email ou senha invalidos.");

        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null)
            throw new DomainException("Usuario nao encontrado.");

        if (!usuario.Ativo)
            throw new DomainException("Usuario desativado. Entre em contato com o administrador.");

        var tokens = await _tokenService.GerarTokensAsync(
            usuario.IdentityUserId,
            usuario.Email,
            usuario.Cargo.ToString(),
            usuario.Id,
            usuario.TenantId,
            cancellationToken);

        usuario.RegistrarAcesso();
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new LoginResponse(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiraEm, usuario.ToResponse());
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenInfo = await _tokenService.ValidarAccessTokenExpiradoAsync(request.AccessToken, cancellationToken);
        if (tokenInfo is null)
            throw new DomainException("Access token invalido.");

        var (identityUserId, _) = tokenInfo.Value;

        var refreshValido = await _tokenService.ValidarRefreshTokenAsync(identityUserId, request.RefreshToken, cancellationToken);
        if (!refreshValido)
            throw new DomainException("Refresh token invalido ou expirado.");

        var usuario = await _usuarioRepository.ObterPorIdentityUserIdAsync(identityUserId, cancellationToken);
        if (usuario is null || !usuario.Ativo)
            throw new DomainException("Usuario nao encontrado ou desativado.");

        await _tokenService.RevogarRefreshTokenAsync(identityUserId, request.RefreshToken, cancellationToken);

        var novosTokens = await _tokenService.GerarTokensAsync(
            usuario.IdentityUserId,
            usuario.Email,
            usuario.Cargo.ToString(),
            usuario.Id,
            usuario.TenantId,
            cancellationToken);

        return new LoginResponse(novosTokens.AccessToken, novosTokens.RefreshToken, novosTokens.ExpiraEm, usuario.ToResponse());
    }

    public async Task<UsuarioResponse> RegistrarAsync(RegistrarUsuarioRequest request, Guid? tenantId, CancellationToken cancellationToken = default)
    {
        if (request.Cargo == CargoUsuario.SuperAdmin)
            throw new DomainException("Nao e possivel registrar um SuperAdmin por esta via.");

        var existente = await _usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (existente is not null)
            throw new DomainException("Ja existe um usuario com este email.");

        var (sucesso, identityUserId, erros) = await _identityService.CriarUsuarioAsync(request.Email, request.Senha, cancellationToken);
        if (!sucesso)
            throw new DomainException($"Falha ao criar usuario: {string.Join(", ", erros)}");

        await _identityService.AdicionarAoCargoAsync(identityUserId, request.Cargo.ToString(), cancellationToken);

        var usuario = Usuario.Criar(request.Nome, request.Email, request.Cargo, identityUserId, tenantId);

        await _usuarioRepository.AdicionarAsync(usuario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return usuario.ToResponse();
    }

    public async Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaRequest request, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            throw new DomainException("Usuario nao encontrado.");

        var alterado = await _identityService.AlterarSenhaAsync(
            usuario.IdentityUserId,
            request.SenhaAtual,
            request.NovaSenha,
            cancellationToken);

        if (!alterado)
            throw new DomainException("Senha atual incorreta.");
    }

    public async Task EsqueciSenhaAsync(EsqueciSenhaRequest request, CancellationToken cancellationToken = default)
    {
        // Nao revelar se o email existe ou nao (seguranca)
        var identityUserId = await _identityService.ObterIdentityUserIdPorEmailAsync(request.Email, cancellationToken);
        if (identityUserId is null)
            return;

        // Gerar token de reset — sera usado futuramente com IEmailService (F6)
        await _identityService.GerarTokenRedefinicaoSenhaAsync(request.Email, cancellationToken);
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaRequest request, CancellationToken cancellationToken = default)
    {
        var redefinido = await _identityService.RedefinirSenhaAsync(request.Email, request.Token, request.NovaSenha, cancellationToken);
        if (!redefinido)
            throw new DomainException("Token de redefinicao invalido ou expirado.");
    }

    public async Task<UsuarioResponse?> ObterUsuarioAtualAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        return usuario?.ToResponse();
    }
}
