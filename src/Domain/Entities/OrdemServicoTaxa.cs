using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entidade filha para lançamento de taxas na OS (ex: frete, visita).
/// </summary>
public sealed class OrdemServicoTaxa
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public string Descricao { get; private set; } = string.Empty;
    public Dinheiro Valor { get; private set; } = Dinheiro.Zero;

    public DateTime CreatedAt { get; private set; }

    private OrdemServicoTaxa() { }

    internal static OrdemServicoTaxa Criar(Guid ordemServicoId, string descricao, decimal valor)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("A Ordem de Serviço deve ser informada.", nameof(ordemServicoId));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição da taxa é obrigatória.", nameof(descricao));

        return new OrdemServicoTaxa
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            Descricao = descricao,
            Valor = new Dinheiro(valor),
            CreatedAt = DateTime.UtcNow
        };
    }
}
