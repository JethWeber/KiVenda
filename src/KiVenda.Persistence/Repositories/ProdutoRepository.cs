using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class ProdutoRepository : IProdutoRepository
{
    private readonly KiVendaDbContext _context;

    public ProdutoRepository(KiVendaDbContext context)
    {
        _context = context;
    }

    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Produtos
            .Include(p => p.Apresentacoes)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default)
    {
        // Fluxo do scanner (Fase 8): o código lido tanto pode ser o
        // código de barras principal do produto como o de uma
        // apresentação específica (ex.: EAN do saco fechado de 1 kg).
        var porProduto = await _context.Produtos
            .Include(p => p.Apresentacoes)
            .FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras, cancellationToken);

        if (porProduto is not null)
        {
            return porProduto;
        }

        var apresentacao = await _context.ApresentacoesProduto
            .FirstOrDefaultAsync(a => a.CodigoBarras == codigoBarras, cancellationToken);

        if (apresentacao is null)
        {
            return null;
        }

        return await _context.Produtos
            .Include(p => p.Apresentacoes)
            .FirstOrDefaultAsync(p => p.Id == apresentacao.ProdutoId, cancellationToken);
    }

    public async Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno, CancellationToken cancellationToken = default)
    {
        return await _context.Produtos
            .Include(p => p.Apresentacoes)
            .FirstOrDefaultAsync(p => p.CodigoInterno == codigoInterno, cancellationToken);
    }

    public async Task<IReadOnlyList<Produto>> ListarAsync(
        string? termoPesquisa = null,
        Guid? categoriaId = null,
        bool apenasAtivos = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Produtos.Include(p => p.Apresentacoes).AsQueryable();

        if (apenasAtivos)
        {
            query = query.Where(p => p.Ativo);
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(termoPesquisa))
        {
            var termo = termoPesquisa.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Nome, $"%{termo}%") ||
                EF.Functions.Like(p.CodigoInterno, $"%{termo}%") ||
                (p.CodigoBarras != null && EF.Functions.Like(p.CodigoBarras, $"%{termo}%")));
        }

        return await query.OrderBy(p => p.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        await _context.Produtos.AddAsync(produto, cancellationToken);
    }
}
