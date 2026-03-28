using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entidade filha que representa uma peça ou produto a ser utilizado na OS.
/// </summary>
public sealed class OrdemServicoProduto
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public string Descricao { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public Dinheiro ValorUnitario { get; private set; } = Dinheiro.Zero;

    public decimal Subtotal => ValorUnitario.Valor * Quantidade;

    public int OrdemExibicao { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private OrdemServicoProduto() { }

    internal static OrdemServicoProduto Criar(Guid ordemServicoId, string descricao, int quantidade, decimal valorUnitario, int ordem)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("A Ordem de Serviço deve ser informada.", nameof(ordemServicoId));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição do produto é obrigatória.", nameof(descricao));

        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser maior que zero.", nameof(quantidade));

        return new OrdemServicoProduto
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            Descricao = descricao,
            Quantidade = quantidade,
            ValorUnitario = new Dinheiro(valorUnitario),
            OrdemExibicao = ordem,
            CreatedAt = DateTime.UtcNow
        };
    }
}
