namespace Domain.Enums;

/// <summary>
/// Niveis de acesso no sistema. SuperAdmin e o dono da plataforma SaaS (sem TenantId).
/// Os demais cargos pertencem a um tenant especifico.
/// </summary>
public enum CargoUsuario
{
    SuperAdmin = 0,
    Admin = 1,
    Gerente = 2,
    Tecnico = 3,
    Atendente = 4
}
