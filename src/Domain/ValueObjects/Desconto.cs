using Domain.Enums;
using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula um desconto numérico em conjunto com a sua modalidade (Percentual ou Valor Fixo).
/// Protege a regra de negócio para que o desconto gerado nunca ultrapasse 100% ou seja aplicado de forma errada.
/// </summary>
public sealed record Desconto
{
    public TipoDesconto Tipo { get; }
    public decimal Valor { get; }

    public Desconto(TipoDesconto tipo, decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("O valor do desconto não pode ser negativo.", nameof(valor));

        if (tipo == TipoDesconto.Percentual && valor > 100)
            throw new ArgumentException("O desconto percentual não pode ser maior que 100%.", nameof(valor));

        Tipo = tipo;
        Valor = Math.Round(valor, 2);
    }

    /// <summary>
    /// Baseado no subtotal bruto da Ordem de Serviço, extrai quanto o cliente irá economizar de fato.
    /// </summary>
    public decimal CalcularValorEfetivo(decimal subtotalOs)
    {
        if (subtotalOs < 0)
            throw new ArgumentException("O subtotal base não pode ser negativo.", nameof(subtotalOs));

        decimal descontoCalculado = Tipo switch
        {
            TipoDesconto.Percentual => Math.Round(subtotalOs * (Valor / 100m), 2),
            TipoDesconto.ValorFixo => Math.Round(Valor, 2),
            _ => throw new InvalidOperationException("Tipo de desconto não suportado.")
        };

        // Validação de negócio (DomainException em vez de ArgumentException)
        if (descontoCalculado > subtotalOs)
            throw new DescontoExcedeTotalException(descontoCalculado, subtotalOs);

        return descontoCalculado;
    }

    public static Desconto Nenhum => new(TipoDesconto.ValorFixo, 0m);
}
