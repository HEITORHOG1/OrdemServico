using Domain.Enums;

namespace Domain.Exceptions;

/// <summary>
/// Lançada quando há tentativa de mudar o status da Ordem de Serviço para um estado não permitido
/// (ex: Tentar voltar de 'Concluida' para 'EmAndamento' sem workflow de reabertura).
/// </summary>
public sealed class StatusTransicaoInvalidaException : DomainException
{
    public StatusOS StatusAtual { get; }
    public StatusOS StatusDesejado { get; }

    public StatusTransicaoInvalidaException(StatusOS atual, StatusOS desejado)
        : base($"A transição de status de '{atual}' para '{desejado}' não é permitida.")
    {
        StatusAtual = atual;
        StatusDesejado = desejado;
    }
}
