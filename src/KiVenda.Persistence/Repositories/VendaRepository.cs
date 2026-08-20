using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class VendaRepository : IVendaRepository
{
    private readonly KiVendaDbContext _context;

    public VendaRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vendas
            .Include(v => v.Itens)
            .Include(v => v.Pagamentos)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Venda>> ListarAsync(
        Guid? utilizadorId = null,
        Guid? clienteId = null,
        DateTime? de = null,
        DateTime? ate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vendas
            .Include(v => v.Itens)
            .Include(v => v.Pagamentos)
            .AsQueryable();

        if (utilizadorId.HasValue)
        {
            query = query.Where(v => v.UtilizadorId == utilizadorId.Value);
        }

        if (clienteId.HasValue)
        {
            query = query.Where(v => v.ClienteId == clienteId.Value);
        }

        if (de.HasValue)
        {
            query = query.Where(v => v.Data >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(v => v.Data <= ate.Value);
        }

        return await query.OrderByDescending(v => v.Data).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Venda venda, CancellationToken cancellationToken = default)
    {
        await _context.Vendas.AddAsync(venda, cancellationToken);
    }
}
