using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Fornecedores;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class FornecedorRepository(KiVendaDbContext context) : IFornecedorRepository
{
    public async Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Fornecedor>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default)
    {
        var query = context.Fornecedores.AsQueryable();
        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var termo = termoPesquisa.Trim();
            query = query.Where(f => EF.Functions.Like(f.Nome, $"%{termo}%")
                || (f.Telefone != null && EF.Functions.Like(f.Telefone, $"%{termo}%"))
                || (f.Nif != null && EF.Functions.Like(f.Nif, $"%{termo}%")));
        }
        return await query.OrderBy(f => f.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Fornecedor fornecedor, CancellationToken cancellationToken = default)
        => await context.Fornecedores.AddAsync(fornecedor, cancellationToken);

    public void Remover(Fornecedor fornecedor) => context.Fornecedores.Remove(fornecedor);
}