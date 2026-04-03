using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.UnitTests.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly AuthService _sut;

    private const string EmailTeste = "usuario@teste.com";
    private const string SenhaTeste = "Senha@123";
    private const string IdentityUserIdTeste = "identity-user-123";
    private static readonly Guid TenantIdTeste = Guid.NewGuid();

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _usuarioRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _identityServiceMock.Object,
            _loggerMock.Object);
    }

    private static Usuario CriarUsuarioValido(bool ativo = true)
    {
        var usuario = Usuario.Criar("Usuario Teste", EmailTeste, CargoUsuario.Admin, IdentityUserIdTeste, TenantIdTeste);
        if (!ativo)
            usuario.Desativar();
        return usuario;
    }

    [Fact]
    public async Task LoginComCredenciaisValidasDeveRetornarTokens()
    {
        var usuario = CriarUsuarioValido();
        var expiraEm = DateTime.UtcNow.AddHours(1);

        _identityServiceMock
            .Setup(x => x.ValidarCredenciaisAsync(EmailTeste, SenhaTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(EmailTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _tokenServiceMock
            .Setup(x => x.GerarTokensAsync(
                IdentityUserIdTeste, EmailTeste, "Admin", usuario.Id, TenantIdTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("access-token", "refresh-token", expiraEm));

        var result = await _sut.LoginAsync(new LoginRequest(EmailTeste, SenhaTeste));

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(expiraEm, result.ExpiraEm);
        Assert.Equal(EmailTeste, result.Usuario.Email);

        _usuarioRepositoryMock.Verify(x => x.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginComSenhaErradaDeveLancarDomainException()
    {
        _identityServiceMock
            .Setup(x => x.ValidarCredenciaisAsync(EmailTeste, "senha-errada", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.LoginAsync(new LoginRequest(EmailTeste, "senha-errada")));

        Assert.Contains("Email ou senha invalidos", ex.Message);
    }

    [Fact]
    public async Task LoginComUsuarioInativoDeveLancarDomainException()
    {
        var usuario = CriarUsuarioValido(ativo: false);

        _identityServiceMock
            .Setup(x => x.ValidarCredenciaisAsync(EmailTeste, SenhaTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(EmailTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.LoginAsync(new LoginRequest(EmailTeste, SenhaTeste)));

        Assert.Contains("desativado", ex.Message);
    }

    [Fact]
    public async Task LoginComUsuarioNaoEncontradoDeveLancarDomainException()
    {
        _identityServiceMock
            .Setup(x => x.ValidarCredenciaisAsync(EmailTeste, SenhaTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(EmailTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.LoginAsync(new LoginRequest(EmailTeste, SenhaTeste)));

        Assert.Contains("nao encontrado", ex.Message);
    }

    [Fact]
    public async Task RefreshTokenComTokenValidoDeveRetornarNovosTokens()
    {
        var usuario = CriarUsuarioValido();
        var expiraEm = DateTime.UtcNow.AddHours(1);

        _tokenServiceMock
            .Setup(x => x.ValidarAccessTokenExpiradoAsync("old-access", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserIdTeste, EmailTeste));

        _tokenServiceMock
            .Setup(x => x.ValidarRefreshTokenAsync(IdentityUserIdTeste, "old-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdentityUserIdAsync(IdentityUserIdTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _tokenServiceMock
            .Setup(x => x.GerarTokensAsync(
                IdentityUserIdTeste, EmailTeste, "Admin", usuario.Id, TenantIdTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("new-access", "new-refresh", expiraEm));

        var result = await _sut.RefreshTokenAsync(new RefreshTokenRequest("old-access", "old-refresh"));

        Assert.Equal("new-access", result.AccessToken);
        Assert.Equal("new-refresh", result.RefreshToken);

        _tokenServiceMock.Verify(
            x => x.RevogarRefreshTokenAsync(IdentityUserIdTeste, "old-refresh", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenComAccessTokenInvalidoDeveLancarDomainException()
    {
        _tokenServiceMock
            .Setup(x => x.ValidarAccessTokenExpiradoAsync("invalid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string, string)?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RefreshTokenAsync(new RefreshTokenRequest("invalid", "refresh")));

        Assert.Contains("Access token invalido", ex.Message);
    }

    [Fact]
    public async Task RefreshTokenComRefreshTokenRevogadoDeveLancarDomainException()
    {
        _tokenServiceMock
            .Setup(x => x.ValidarAccessTokenExpiradoAsync("access", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserIdTeste, EmailTeste));

        _tokenServiceMock
            .Setup(x => x.ValidarRefreshTokenAsync(IdentityUserIdTeste, "revoked-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RefreshTokenAsync(new RefreshTokenRequest("access", "revoked-refresh")));

        Assert.Contains("Refresh token invalido", ex.Message);
    }

    [Fact]
    public async Task RegistrarComDadosValidosDeveCriarUsuario()
    {
        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(EmailTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        _identityServiceMock
            .Setup(x => x.CriarUsuarioAsync(EmailTeste, SenhaTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, IdentityUserIdTeste, Enumerable.Empty<string>()));

        var request = new RegistrarUsuarioRequest("Novo Usuario", EmailTeste, SenhaTeste, SenhaTeste, CargoUsuario.Tecnico);
        var result = await _sut.RegistrarAsync(request, TenantIdTeste);

        Assert.Equal(EmailTeste, result.Email);
        Assert.Equal("Novo Usuario", result.Nome);

        _identityServiceMock.Verify(
            x => x.AdicionarAoCargoAsync(IdentityUserIdTeste, "Tecnico", It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarComEmailDuplicadoDeveLancarDomainException()
    {
        var existente = CriarUsuarioValido();
        _usuarioRepositoryMock
            .Setup(x => x.ObterPorEmailAsync(EmailTeste, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var request = new RegistrarUsuarioRequest("Outro", EmailTeste, SenhaTeste, SenhaTeste, CargoUsuario.Tecnico);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RegistrarAsync(request, TenantIdTeste));

        Assert.Contains("Ja existe", ex.Message);
    }

    [Fact]
    public async Task RegistrarComCargoSuperAdminDeveLancarDomainException()
    {
        var request = new RegistrarUsuarioRequest("Admin", EmailTeste, SenhaTeste, SenhaTeste, CargoUsuario.SuperAdmin);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RegistrarAsync(request, null));

        Assert.Contains("SuperAdmin", ex.Message);
    }

    [Fact]
    public async Task AlterarSenhaComSenhaCorretaDeveAlterar()
    {
        var usuario = CriarUsuarioValido();

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _identityServiceMock
            .Setup(x => x.AlterarSenhaAsync(IdentityUserIdTeste, "atual", "nova", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.AlterarSenhaAsync(usuario.Id, new AlterarSenhaRequest("atual", "nova", "nova"));
    }

    [Fact]
    public async Task AlterarSenhaComSenhaErradaDeveLancarDomainException()
    {
        var usuario = CriarUsuarioValido();

        _usuarioRepositoryMock
            .Setup(x => x.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _identityServiceMock
            .Setup(x => x.AlterarSenhaAsync(IdentityUserIdTeste, "errada", "nova", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            _sut.AlterarSenhaAsync(usuario.Id, new AlterarSenhaRequest("errada", "nova", "nova")));

        Assert.Contains("incorreta", ex.Message);
    }
}
