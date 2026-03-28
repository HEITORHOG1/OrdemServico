using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

/// <summary>
/// Value Object que encapsula e valida a formatação de um número de Ordem de Serviço.
/// Formato esperado: OS-YYYYMMDD-XXXX (Ex: OS-20260307-0001)
/// </summary>
public sealed record NumeroOS
{
    public string Valor { get; }

    private static readonly Regex FormatoValido = new(@"^OS-\d{8}-\d{4}$", RegexOptions.Compiled);

    private NumeroOS(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("O número da Ordem de Serviço é obrigatório.", nameof(valor));

        if (!FormatoValido.IsMatch(valor))
            throw new ArgumentException("Número da OS fora do padrão esperado (OS-YYYYMMDD-XXXX).", nameof(valor));

        Valor = valor;
    }

    /// <summary>
    /// Gera um novo número válido baseado na data e num sequencial garantido pelo banco de dados.
    /// </summary>
    public static NumeroOS Gerar(DateTime dataReferencia, int sequencialDiario)
    {
        if (sequencialDiario <= 0 || sequencialDiario > 9999)
            throw new ArgumentException("O sequencial diário deve estar entre 1 e 9999.", nameof(sequencialDiario));

        var dataFormatada = dataReferencia.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        var sequencialFormatado = sequencialDiario.ToString("D4", System.Globalization.CultureInfo.InvariantCulture);

        return new NumeroOS($"OS-{dataFormatada}-{sequencialFormatado}");
    }

    /// <summary>
    /// Restaura um número existente a partir de uma persistência prévia.
    /// </summary>
    public static NumeroOS Parse(string valor) => new(valor);

    public override string ToString() => Valor;

    public static implicit operator string(NumeroOS numero) => numero.Valor;
}
