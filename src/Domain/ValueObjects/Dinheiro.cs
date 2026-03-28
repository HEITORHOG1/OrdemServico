namespace Domain.ValueObjects;

/// <summary>
/// Value Object que representa um valor monetário no sistema.
/// Garante que valores nunca sejam negativos e aplica arredondamento de 2 casas decimais.
/// </summary>
public sealed record Dinheiro
{
    public decimal Valor { get; }

    public Dinheiro(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("O valor monetário não pode ser negativo.", nameof(valor));

        Valor = Math.Round(valor, 2);
    }

    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => new(a.Valor + b.Valor);

    public static Dinheiro operator -(Dinheiro a, Dinheiro b)
    {
        if (a.Valor < b.Valor)
            throw new ArgumentException("A subtração resultaria em um valor negativo, o que não é permitido para Dinheiro.");

        return new Dinheiro(a.Valor - b.Valor);
    }

    public static implicit operator decimal(Dinheiro dinheiro) => dinheiro.Valor;
    public static explicit operator Dinheiro(decimal valor) => new(valor);

    public static Dinheiro Zero => new(0m);

    public override string ToString() => Valor.ToString("C", System.Globalization.CultureInfo.CurrentCulture);
}
