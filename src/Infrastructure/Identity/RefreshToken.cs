namespace Infrastructure.Identity;

/// <summary>
/// Entidade de infraestrutura para persistir refresh tokens.
/// Nao faz parte do Domain — e um detalhe de implementacao do JWT.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? RevogadoEm { get; set; }

    public bool EstaAtivo => RevogadoEm is null && ExpiraEm > DateTime.UtcNow;
}
