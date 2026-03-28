using Web.Models.Responses;
using Web.State;

namespace Web.Models.Mappings;

public static class UiMappingExtensions
{
    public static OrdemServicoListItemState ToListItemState(this OrdemServicoResumoResponseModel response)
    {
        return new OrdemServicoListItemState(
            response.Id,
            response.Numero,
            response.Status,
            response.ClienteId,
            response.Defeito,
            response.ValorTotal,
            response.CreatedAt);
    }

    public static SelectOptionModel<Guid> ToOption(this ClienteResponseModel response)
    {
        return new SelectOptionModel<Guid>(response.Id, response.Nome);
    }

    public static SelectOptionModel<Guid> ToOption(this EquipamentoResponseModel response)
    {
        var label = string.Join(" - ", new[] { response.Tipo, response.Marca, response.Modelo }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return new SelectOptionModel<Guid>(response.Id, label);
    }
}
