using Web.Models.Responses;

namespace Web.State;

public sealed class ClienteFlowState
{
    public Guid? UltimoClienteId { get; private set; }
    public string? UltimoClienteNome { get; private set; }
    public DateTime? UltimoCadastroEm { get; private set; }

    public void SetUltimoCliente(ClienteResponseModel cliente)
    {
        UltimoClienteId = cliente.Id;
        UltimoClienteNome = cliente.Nome;
        UltimoCadastroEm = DateTime.UtcNow;
    }
}
