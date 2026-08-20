using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class MovimentoStockRepository : IMovimentoStockRepository
{
    private readonly KiVendaDbContext _context;

    public MovimentoStockRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(MovimentoStock movimento, CancellationToken cancellationToken = default)
    {
        await _context.MovimentosStock.AddAsync(movimento, cancellationToken);
    }

    public async Task<IReadOnlyList<MovimentoStock>> ListarPorProdutoAsync(
        Guid produtoId,
        DateTime? de = null,
        DateTime? ate = null,
        int pagina = 1,
        int tamanhoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MovimentosStock.Where(m => m.ProdutoId == produtoId);

        if (de.HasValue)
        {
            query = query.Where(m => m.Data >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(m => m.Data <= ate.Value);
        }

        return await query
            .OrderByDescending(m => m.Data)
            .Skip(Math.Max(0, pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MovimentoStock>> ListarTodosPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken = default)
    {
        return await _context.MovimentosStock
            .Where(m => m.ProdutoId == produtoId)
            .OrderBy(m => m.Data)
            .ToListAsync(cancellationToken);
    }
}
