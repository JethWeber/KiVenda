using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly KiVendaDbContext _context;

    public CategoriaRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categorias.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        await _context.Categorias.AddAsync(categoria, cancellationToken);
    }
}

internal sealed class UnidadeMedidaRepository : IUnidadeMedidaRepository
{
    private readonly KiVendaDbContext _context;

    public UnidadeMedidaRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<UnidadeMedida?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UnidadesMedida.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<UnidadeMedida?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.UnidadesMedida.FirstOrDefaultAsync(u => u.Codigo == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<UnidadeMedida>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UnidadesMedida.OrderBy(u => u.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(UnidadeMedida unidadeMedida, CancellationToken cancellationToken = default)
    {
        await _context.UnidadesMedida.AddAsync(unidadeMedida, cancellationToken);
    }
}
