using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Notificacoes;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class NotificacaoRepository : INotificacaoRepository
{
    private readonly KiVendaDbContext _context;
    public NotificacaoRepository(KiVendaDbContext context) => _context = context;

    public async Task<IReadOnlyList<Notificacao>> ListarPorUtilizadorAsync(Guid utilizadorId, int limite = 100, CancellationToken cancellationToken = default)
    {
        limite = Math.Clamp(limite, 1, 500);
        return await _context.Notificacoes.Where(n => n.UtilizadorId == utilizadorId)
            .OrderByDescending(n => n.DataCriacao).Take(limite).ToListAsync(cancellationToken);
    }

    public Task<Notificacao?> ObterUltimaPorTipoAsync(Guid utilizadorId, string tipo, CancellationToken cancellationToken = default) =>
        _context.Notificacoes.Where(n => n.UtilizadorId == utilizadorId && n.Tipo == tipo)
            .OrderByDescending(n => n.DataCriacao).FirstOrDefaultAsync(cancellationToken);

    public async Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default) =>
        await _context.Notificacoes.AddAsync(notificacao, cancellationToken);
}