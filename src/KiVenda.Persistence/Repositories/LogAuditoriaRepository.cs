using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Auditoria;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class LogAuditoriaRepository : ILogAuditoriaRepository
{
    private readonly KiVendaDbContext _context;

    public LogAuditoriaRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(LogAuditoria log, CancellationToken cancellationToken = default)
    {
        await _context.LogsAuditoria.AddAsync(log, cancellationToken);
    }

    public async Task<IReadOnlyList<LogAuditoria>> ListarAsync(
        Guid? utilizadorId = null,
        string? entidadeAfetada = null,
        DateTime? de = null,
        DateTime? ate = null,
        int pagina = 1,
        int tamanhoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LogsAuditoria.AsQueryable();

        if (utilizadorId.HasValue)
        {
            query = query.Where(l => l.UtilizadorId == utilizadorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entidadeAfetada))
        {
            query = query.Where(l => l.EntidadeAfetada == entidadeAfetada);
        }

        if (de.HasValue)
        {
            query = query.Where(l => l.DataHora >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(l => l.DataHora <= ate.Value);
        }

        return await query
            .OrderByDescending(l => l.DataHora)
            .Skip(Math.Max(0, pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }
}
