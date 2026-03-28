using Application.DTOs.Equipamentos;

namespace Application.Interfaces;

public interface IEquipamentoService
{
    Task<EquipamentoResponse> CriarAsync(CriarEquipamentoRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<EquipamentoResponse>> ListarPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
