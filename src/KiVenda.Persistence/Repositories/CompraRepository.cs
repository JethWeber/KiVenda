using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Compras;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class CompraRepository : ICompraRepository
{
    private readonly KiVendaDbContext _context;

    public CompraRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Compra?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Compras
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Compra>> ListarAsync(
        Guid? fornecedorId = null,
        DateTime? de = null,
        DateTime? ate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Compras.Include(c => c.Itens).AsQueryable();

        if (fornecedorId.HasValue)
        {
            query = query.Where(c => c.FornecedorId == fornecedorId.Value);
        }

        if (de.HasValue)
        {
            query = query.Where(c => c.Data >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(c => c.Data <= ate.Value);
        }

        return await query.OrderByDescending(c => c.Data).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Compra compra, CancellationToken cancellationToken = default)
    {
        await _context.Compras.AddAsync(compra, cancellationToken);
    }
}
