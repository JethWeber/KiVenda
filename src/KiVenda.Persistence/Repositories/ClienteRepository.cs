using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Clientes;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class ClienteRepository(KiVendaDbContext context) : IClienteRepository
{
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default)
    {
        var query = context.Clientes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var termo = termoPesquisa.Trim();
            query = query.Where(c => EF.Functions.Like(c.Nome, $"%{termo}%")
                || (c.Telefone != null && EF.Functions.Like(c.Telefone, $"%{termo}%"))
                || (c.Nif != null && EF.Functions.Like(c.Nif, $"%{termo}%")));
        }
        return await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await context.Clientes.AddAsync(cliente, cancellationToken);

    public void Remover(Cliente cliente) => context.Clientes.Remove(cliente);
}