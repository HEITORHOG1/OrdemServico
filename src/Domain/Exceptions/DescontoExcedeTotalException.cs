namespace Domain.Exceptions;

/// <summary>
/// Lançada quando o valor do desconto (calculado ou em valor fixo) supera
/// o total de serviços e produtos da Ordem de Serviço, o que resultaria num valor geral negativo.
/// </summary>
public sealed class DescontoExcedeTotalException : DomainException
{
    public decimal ValorDesconto { get; }
    public decimal SubtotalOS { get; }

    public DescontoExcedeTotalException(decimal valorDesconto, decimal subtotalOs)
        : base($"O valor do desconto ({valorDesconto:C}) não pode exceder o subtotal da Ordem de Serviço ({subtotalOs:C}).")
    {
        ValorDesconto = valorDesconto;
        SubtotalOS = subtotalOs;
    }
}
