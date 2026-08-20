using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class UtilizadorRepository : IUtilizadorRepository
{
    private readonly KiVendaDbContext _context;

    public UtilizadorRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Utilizador?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Utilizadores.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Utilizador?> ObterPorNomeUtilizadorAsync(string nomeUtilizador, CancellationToken cancellationToken = default)
    {
        return await _context.Utilizadores.FirstOrDefaultAsync(u => u.NomeUtilizador == nomeUtilizador, cancellationToken);
    }

    public async Task<IReadOnlyList<Utilizador>> ListarAsync(bool apenasAtivos = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Utilizadores.AsQueryable();

        if (apenasAtivos)
        {
            query = query.Where(u => u.Ativo);
        }

        return await query.OrderBy(u => u.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Utilizador utilizador, CancellationToken cancellationToken = default)
    {
        await _context.Utilizadores.AddAsync(utilizador, cancellationToken);
    }
}
