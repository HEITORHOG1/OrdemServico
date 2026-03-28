namespace Domain.Entities;

/// <summary>
/// Entidade filha para histórico e registros internos do laboratório (não deve sair no PDF final).
/// </summary>
public sealed class OrdemServicoAnotacao
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public string Texto { get; private set; } = string.Empty;
    public string Autor { get; private set; } = string.Empty; // Nome ou ID de quem registrou

    public DateTime CreatedAt { get; private set; }

    private OrdemServicoAnotacao() { }

    internal static OrdemServicoAnotacao Criar(Guid ordemServicoId, string texto, string autor)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("A Ordem de Serviço deve ser informada.", nameof(ordemServicoId));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("O texto da anotação não pode ser vazio.", nameof(texto));

        if (string.IsNullOrWhiteSpace(autor))
            throw new ArgumentException("O autor da anotação é obrigatório.", nameof(autor));

        return new OrdemServicoAnotacao
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            Texto = texto,
            Autor = autor,
            CreatedAt = DateTime.UtcNow
        };
    }
}
