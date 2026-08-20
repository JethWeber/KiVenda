using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Fornecedores;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class FornecedorRepository : IFornecedorRepository
{
    private readonly KiVendaDbContext _context;

    public FornecedorRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Fornecedor>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Fornecedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var termo = termoPesquisa.Trim();
            query = query.Where(f => EF.Functions.Like(f.Nome, $"%{termo}%"));
        }

        return await query.OrderBy(f => f.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Fornecedor fornecedor, CancellationToken cancellationToken = default)
    {
        await _context.Fornecedores.AddAsync(fornecedor, cancellationToken);
    }
}
