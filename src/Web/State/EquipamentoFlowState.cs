namespace Web.State;

public sealed class EquipamentoFlowState
{
    public Guid? ClienteIdContexto { get; private set; }
    public Guid? EquipamentoSelecionadoId { get; private set; }
    public string? EquipamentoSelecionadoDescricao { get; private set; }
    public DateTime? UltimaSelecaoEm { get; private set; }

    public void DefinirClienteContexto(Guid clienteId)
    {
        ClienteIdContexto = clienteId;
    }

    public void SelecionarEquipamento(Guid equipamentoId, string descricao)
    {
        EquipamentoSelecionadoId = equipamentoId;
        EquipamentoSelecionadoDescricao = descricao;
        UltimaSelecaoEm = DateTime.UtcNow;
    }
}
