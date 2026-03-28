using Application.DTOs.Clientes;

namespace Application.Interfaces;

public interface IClienteService
{
    Task<ClienteResponse> CriarAsync(CriarClienteRequest request, CancellationToken cancellationToken = default);
    Task<ClienteResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClienteResponse>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default);
}
