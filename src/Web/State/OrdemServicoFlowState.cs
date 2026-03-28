using Web.Models.Responses;
using Web.Models;

namespace Web.State;

public sealed class OrdemServicoFlowState
{
    public Guid? UltimaOsId { get; private set; }
    public string? UltimoNumeroOs { get; private set; }
    public StatusOsModel? UltimoStatusOs { get; private set; }
    public DateTime? UltimaCriacaoEm { get; private set; }

    public void SetUltimaOs(OrdemServicoResponseModel os)
    {
        UltimaOsId = os.Id;
        UltimoNumeroOs = os.Numero;
        UltimoStatusOs = os.Status;
        UltimaCriacaoEm = DateTime.UtcNow;
    }
}
