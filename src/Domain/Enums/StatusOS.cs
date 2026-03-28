namespace Domain.Enums;

/// <summary>
/// Representa os estados possíveis de uma Ordem de Serviço ao longo do seu ciclo de vida.
/// Os valores inteiros são explícitos para garantir estabilidade na persistência.
/// </summary>
public enum StatusOS
{
    /// <summary>OS criada, ainda sendo preenchida. Pode ser editada livremente.</summary>
    Rascunho = 0,

    /// <summary>Enviada ao cliente para aprovação. Campos financeiros travados.</summary>
    Orcamento = 1,

    /// <summary>Cliente concordou com o orçamento. Serviço pode ser iniciado.</summary>
    Aprovada = 2,

    /// <summary>Cliente não aceitou o orçamento. OS encerrada.</summary>
    Rejeitada = 3,

    /// <summary>Serviço em execução pelo técnico.</summary>
    EmAndamento = 4,

    /// <summary>Serviço pausado por falta de peça/produto.</summary>
    AguardandoPeca = 5,

    /// <summary>Serviço finalizado. Aguardando retirada/entrega ao cliente.</summary>
    Concluida = 6,

    /// <summary>Equipamento devolvido ao cliente. OS encerrada com sucesso.</summary>
    Entregue = 7
}
