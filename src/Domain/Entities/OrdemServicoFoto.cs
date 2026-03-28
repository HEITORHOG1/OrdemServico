namespace Domain.Entities;

/// <summary>
/// Entidade filha representando as fotos anexadas (ex: equipamento avariado, garantias).
/// </summary>
public sealed class OrdemServicoFoto
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public string Url { get; private set; } = string.Empty;
    public string? Legenda { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private OrdemServicoFoto() { }

    internal static OrdemServicoFoto Criar(Guid ordemServicoId, string url, string? legenda)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("A Ordem de Serviço deve ser informada.", nameof(ordemServicoId));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("A URL da foto é obrigatória.", nameof(url));

        return new OrdemServicoFoto
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            Url = url,
            Legenda = legenda,
            CreatedAt = DateTime.UtcNow
        };
    }
}
