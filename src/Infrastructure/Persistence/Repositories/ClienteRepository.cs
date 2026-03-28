using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }

    public Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        _context.Clientes.Update(cliente);
        return Task.CompletedTask;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<bool> ExistePorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes.AnyAsync(x => x.Documento == documento, cancellationToken);
    }

    public async Task<IEnumerable<Cliente>> BuscarPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .Where(x => EF.Functions.Like(x.Nome, $"%{nome}%"))
            .OrderBy(x => x.Nome)
            .Take(20)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
