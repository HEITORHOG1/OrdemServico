namespace Domain.Enums;

/// <summary>
/// Define como o desconto é aplicado na Ordem de Serviço.
/// </summary>
public enum TipoDesconto
{
    /// <summary>Desconto calculado como porcentagem do subtotal (0–100%).</summary>
    Percentual = 0,

    /// <summary>Desconto aplicado como valor fixo em reais.</summary>
    ValorFixo = 1
}
