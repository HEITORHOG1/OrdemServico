using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.UnitTests.Entities;

public sealed class UsuarioTests
{
    private const string NomeValido = "Joao Silva";
    private const string EmailValido = "joao@email.com";
    private const string IdentityUserIdValido = "identity-user-id-123";
    private static readonly Guid TenantIdValido = Guid.NewGuid();

    [Fact]
    public void CriarComCargoSuperAdminTenantIdDeveSerNull()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.SuperAdmin, IdentityUserIdValido, null);

        Assert.Equal(CargoUsuario.SuperAdmin, usuario.Cargo);
        Assert.Null(usuario.TenantId);
        Assert.True(usuario.Ativo);
        Assert.True(usuario.EhSuperAdmin());
    }

    [Fact]
    public void CriarComCargoSuperAdminComTenantIdDeveLancarDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Usuario.Criar(NomeValido, EmailValido, CargoUsuario.SuperAdmin, IdentityUserIdValido, TenantIdValido));

        Assert.Contains("SuperAdmin nao pode pertencer a um tenant", ex.Message);
    }

    [Fact]
    public void CriarComCargoAdminTenantIdDeveSerObrigatorio()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);

        Assert.Equal(CargoUsuario.Admin, usuario.Cargo);
        Assert.Equal(TenantIdValido, usuario.TenantId);
    }

    [Fact]
    public void CriarComCargoAdminSemTenantIdDeveLancarDomainException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, null));

        Assert.Contains("devem pertencer a um tenant", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarComNomeInvalidoDeveLancarArgumentException(string? nome)
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar(nome!, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarComEmailInvalidoDeveLancarArgumentException(string? email)
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar(NomeValido, email!, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarComIdentityUserIdInvalidoDeveLancarArgumentException(string? identityId)
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, identityId!, TenantIdValido));
    }

    [Fact]
    public void RegistrarAcessoDeveAtualizarUltimoAcesso()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);
        var antes = usuario.UltimoAcesso;

        usuario.RegistrarAcesso();

        Assert.NotNull(usuario.UltimoAcesso);
        Assert.NotEqual(antes, usuario.UltimoAcesso);
    }

    [Fact]
    public void AlterarCargoDeUsuarioNormalDeveAlterar()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Tecnico, IdentityUserIdValido, TenantIdValido);

        usuario.AlterarCargo(CargoUsuario.Gerente);

        Assert.Equal(CargoUsuario.Gerente, usuario.Cargo);
    }

    [Fact]
    public void AlterarCargoDeSuperAdminDeveLancarDomainException()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.SuperAdmin, IdentityUserIdValido, null);

        Assert.Throws<DomainException>(() => usuario.AlterarCargo(CargoUsuario.Admin));
    }

    [Fact]
    public void AlterarCargoParaSuperAdminDeveLancarDomainException()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);

        Assert.Throws<DomainException>(() => usuario.AlterarCargo(CargoUsuario.SuperAdmin));
    }

    [Fact]
    public void DesativarUsuarioNormalDeveDesativar()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Tecnico, IdentityUserIdValido, TenantIdValido);

        usuario.Desativar();

        Assert.False(usuario.Ativo);
    }

    [Fact]
    public void DesativarSuperAdminDeveLancarDomainException()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.SuperAdmin, IdentityUserIdValido, null);

        Assert.Throws<DomainException>(() => usuario.Desativar());
    }

    [Fact]
    public void ReativarDeveReativarUsuario()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Tecnico, IdentityUserIdValido, TenantIdValido);
        usuario.Desativar();

        usuario.Reativar();

        Assert.True(usuario.Ativo);
    }

    [Fact]
    public void EhSuperAdminComCargoSuperAdminDeveRetornarTrue()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.SuperAdmin, IdentityUserIdValido, null);

        Assert.True(usuario.EhSuperAdmin());
    }

    [Fact]
    public void EhSuperAdminComOutroCargoDeveRetornarFalse()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);

        Assert.False(usuario.EhSuperAdmin());
    }

    [Fact]
    public void PertenceAoTenantComTenantCorretoDeveRetornarTrue()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);

        Assert.True(usuario.PertenceAoTenant(TenantIdValido));
    }

    [Fact]
    public void PertenceAoTenantComTenantDiferenteDeveRetornarFalse()
    {
        var usuario = Usuario.Criar(NomeValido, EmailValido, CargoUsuario.Admin, IdentityUserIdValido, TenantIdValido);

        Assert.False(usuario.PertenceAoTenant(Guid.NewGuid()));
    }
}
