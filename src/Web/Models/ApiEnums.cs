namespace Web.Models;

public enum StatusOsModel
{
    Rascunho = 0,
    Orcamento = 1,
    Aprovada = 2,
    Rejeitada = 3,
    EmAndamento = 4,
    AguardandoPeca = 5,
    Concluida = 6,
    Entregue = 7
}

public enum TipoDescontoModel
{
    Percentual = 0,
    ValorFixo = 1
}

public enum MeioPagamentoModel
{
    Dinheiro = 0,
    Pix = 1,
    CartaoCredito = 2,
    CartaoDebito = 3,
    Boleto = 4,
    Transferencia = 5
}
