using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Clientes;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class ClienteRepository : IClienteRepository
{
    private readonly KiVendaDbContext _context;

    public ClienteRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var termo = termoPesquisa.Trim();
            query = query.Where(c => EF.Functions.Like(c.Nome, $"%{termo}%"));
        }

        return await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }
}
