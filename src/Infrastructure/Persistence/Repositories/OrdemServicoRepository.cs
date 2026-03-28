using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Persistence.Repositories;

public sealed class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly AppDbContext _context;

    public OrdemServicoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        await _context.OrdensServico.AddAsync(ordemServico, cancellationToken);
    }

    public Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(ordemServico);

        // The aggregate is usually loaded and tracked in the same DbContext.
        // Calling Update on the whole graph can mark new child items as Modified,
        // which triggers concurrency failures when EF tries to update rows that do not exist yet.
        if (entry.State == EntityState.Detached)
        {
            _context.OrdensServico.Attach(ordemServico);
            _context.Entry(ordemServico).State = EntityState.Modified;
        }

        // New aggregate children can show up as Detached when added through backing fields.
        // Explicitly mark them as Added so EF emits INSERT instead of UPDATE.
        MarkIfDetachedAsAdded(ordemServico.Servicos);
        MarkIfDetachedAsAdded(ordemServico.Produtos);
        MarkIfDetachedAsAdded(ordemServico.Taxas);
        MarkIfDetachedAsAdded(ordemServico.Pagamentos);
        MarkIfDetachedAsAdded(ordemServico.Fotos);
        MarkIfDetachedAsAdded(ordemServico.Anotacoes);

        return Task.CompletedTask;
    }

    private void MarkIfDetachedAsAdded<T>(IEnumerable<T> entities)
        where T : class
    {
        foreach (var entity in entities)
        {
            var childEntry = _context.Entry(entity);
            if (childEntry.State is EntityState.Detached or EntityState.Modified)
            {
                childEntry.State = EntityState.Added;
            }
        }
    }

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.OrdensServico
            .Include(x => x.Servicos)
            .Include(x => x.Produtos)
            .Include(x => x.Taxas)
            .Include(x => x.Pagamentos)
            .Include(x => x.Fotos)
            .Include(x => x.Anotacoes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OrdemServico>> ListarPaginadoAsync(int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        return await _context.OrdensServico
            .Include(x => x.Servicos)
            .Include(x => x.Produtos)
            .Include(x => x.Taxas)
            .Include(x => x.Pagamentos)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.OrdensServico.CountAsync(cancellationToken);
    }

    public async Task<int> ObterProximoSequencialNoDiaAsync(DateTime dataReferencia, CancellationToken cancellationToken = default)
    {
        // Exemplo simplificado. Num sistema real muito acessado, pode usar um Sequence do banco
        // ou criar uma tabela de controle de numeração. Usaremos count no dia para o MVP.

        var dateOnly = DateOnly.FromDateTime(dataReferencia);
        var qteHoje = await _context.OrdensServico
            .Where(x => x.CreatedAt.Date == dataReferencia.Date)
            .CountAsync(cancellationToken);

        return qteHoje + 1;
    }
}
