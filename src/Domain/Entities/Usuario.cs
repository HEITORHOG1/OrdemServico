using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

/// <summary>
/// Entidade de dominio que representa um usuario do sistema.
/// Separada do IdentityUser (infra) — vinculada via IdentityUserId.
/// SuperAdmin tem TenantId null. Demais cargos exigem TenantId.
/// </summary>
public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string IdentityUserId { get; private set; } = string.Empty;
    public Guid? TenantId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public CargoUsuario Cargo { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime? UltimoAcesso { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Usuario() { }

    public static Usuario Criar(
        string nome,
        string email,
        CargoUsuario cargo,
        string identityUserId,
        Guid? tenantId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuario e obrigatorio.", nameof(nome));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email do usuario e obrigatorio.", nameof(email));

        if (string.IsNullOrWhiteSpace(identityUserId))
            throw new ArgumentException("O IdentityUserId e obrigatorio.", nameof(identityUserId));

        if (cargo == CargoUsuario.SuperAdmin && tenantId.HasValue)
            throw new DomainException("SuperAdmin nao pode pertencer a um tenant.");

        if (cargo != CargoUsuario.SuperAdmin && !tenantId.HasValue)
            throw new DomainException("Usuarios que nao sao SuperAdmin devem pertencer a um tenant.");

        var agora = DateTime.UtcNow;

        return new Usuario
        {
            Id = Guid.NewGuid(),
            IdentityUserId = identityUserId,
            TenantId = tenantId,
            Nome = nome,
            Email = email,
            Cargo = cargo,
            Ativo = true,
            CreatedAt = agora,
            UpdatedAt = agora
        };
    }

    public bool EhSuperAdmin() => Cargo == CargoUsuario.SuperAdmin;

    public bool PertenceAoTenant(Guid tenantId) => TenantId == tenantId;

    public void RegistrarAcesso()
    {
        UltimoAcesso = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AlterarCargo(CargoUsuario novoCargo)
    {
        if (Cargo == CargoUsuario.SuperAdmin)
            throw new DomainException("Nao e possivel alterar o cargo de um SuperAdmin.");

        if (novoCargo == CargoUsuario.SuperAdmin)
            throw new DomainException("Nao e possivel promover um usuario a SuperAdmin.");

        Cargo = novoCargo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desativar()
    {
        if (Cargo == CargoUsuario.SuperAdmin)
            throw new DomainException("Nao e possivel desativar um SuperAdmin.");

        Ativo = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativo = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AtualizarDados(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do usuario e obrigatorio.", nameof(nome));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O email do usuario e obrigatorio.", nameof(email));

        Nome = nome;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
