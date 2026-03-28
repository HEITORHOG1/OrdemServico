using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Entidade filha representando o histórico de recebimentos do serviço prestado na OS.
/// </summary>
public sealed class OrdemServicoPagamento
{
    public Guid Id { get; private set; }
    public Guid OrdemServicoId { get; private set; }

    public MeioPagamento MeioPagamento { get; private set; }
    public Dinheiro Valor { get; private set; } = Dinheiro.Zero;

    public DateTime DataPagamento { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OrdemServicoPagamento() { }

    internal static OrdemServicoPagamento Criar(Guid ordemServicoId, MeioPagamento meio, decimal valor, DateTime dataPagamento)
    {
        if (ordemServicoId == Guid.Empty)
            throw new ArgumentException("A Ordem de Serviço deve ser informada.", nameof(ordemServicoId));

        var dinheiro = new Dinheiro(valor);
        if (dinheiro.Valor == 0)
            throw new ArgumentException("O valor do pagamento deve ser maior que zero.", nameof(valor));

        return new OrdemServicoPagamento
        {
            Id = Guid.NewGuid(),
            OrdemServicoId = ordemServicoId,
            MeioPagamento = meio,
            Valor = dinheiro,
            DataPagamento = dataPagamento,
            CreatedAt = DateTime.UtcNow
        };
    }
}
