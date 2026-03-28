using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class EquipamentoRepository : IEquipamentoRepository
{
    private readonly AppDbContext _context;

    public EquipamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Equipamento equipamento, CancellationToken cancellationToken = default)
    {
        await _context.Equipamentos.AddAsync(equipamento, cancellationToken);
    }

    public Task AtualizarAsync(Equipamento equipamento, CancellationToken cancellationToken = default)
    {
        _context.Equipamentos.Update(equipamento);
        return Task.CompletedTask;
    }

    public async Task<Equipamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Equipamentos.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Equipamento>> ListarPorClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Equipamentos
            .Where(x => x.ClienteId == clienteId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
