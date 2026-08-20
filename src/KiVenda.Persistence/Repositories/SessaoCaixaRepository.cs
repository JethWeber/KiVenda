using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Caixa;
using KiVenda.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class SessaoCaixaRepository : ISessaoCaixaRepository
{
    private readonly KiVendaDbContext _context;

    public SessaoCaixaRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<SessaoCaixa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SessoesCaixa
            .Include(s => s.Movimentos)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SessaoCaixa?> ObterAbertaAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SessoesCaixa
            .Include(s => s.Movimentos)
            .FirstOrDefaultAsync(s => s.Estado == EstadoSessaoCaixa.Aberta, cancellationToken);
    }

    public async Task<IReadOnlyList<SessaoCaixa>> ListarAsync(DateTime? de = null, DateTime? ate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SessoesCaixa.Include(s => s.Movimentos).AsQueryable();

        if (de.HasValue)
        {
            query = query.Where(s => s.DataAbertura >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(s => s.DataAbertura <= ate.Value);
        }

        return await query.OrderByDescending(s => s.DataAbertura).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(SessaoCaixa sessaoCaixa, CancellationToken cancellationToken = default)
    {
        await _context.SessoesCaixa.AddAsync(sessaoCaixa, cancellationToken);
    }
}
