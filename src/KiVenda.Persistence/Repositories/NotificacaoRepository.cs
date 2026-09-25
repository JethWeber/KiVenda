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
        return await _context.Notificacoes
            .Where(n => n.UtilizadorId == utilizadorId)
            .OrderByDescending(n => n.DataCriacao)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default) =>
        await _context.Notificacoes.AddAsync(notificacao, cancellationToken);
}
