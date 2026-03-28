using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Repositório para consulta e mutação de registro de Equipamentos do Tomador (Cliente).
/// </summary>
public interface IEquipamentoRepository
{
    Task AdicionarAsync(Equipamento equipamento, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Equipamento equipamento, CancellationToken cancellationToken = default);
    Task<Equipamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipamento>> ListarPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
}
