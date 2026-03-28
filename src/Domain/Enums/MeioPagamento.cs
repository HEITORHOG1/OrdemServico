namespace Domain.Enums;

/// <summary>
/// Meios de pagamento aceitos para uma Ordem de Serviço.
/// Uma OS pode ter pagamento dividido entre múltiplos meios.
/// </summary>
public enum MeioPagamento
{
    Dinheiro = 0,
    PIX = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Boleto = 4,
    Transferencia = 5
}
