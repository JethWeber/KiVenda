using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Clientes;
using KiVenda.Core.Compras;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using SessaoCaixaEntity = KiVenda.Core.Caixa.SessaoCaixa;

namespace KiVenda.Application.Tests.Fakes;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    private readonly InMemoryDatabase _db;

    public InMemoryUnitOfWork(InMemoryDatabase db)
    {
        _db = db;
    }

    public IProdutoRepository Produtos => new FakeProdutoRepository(_db);

    public ICategoriaRepository Categorias => new FakeCategoriaRepository(_db);

    public IUnidadeMedidaRepository UnidadesMedida => new FakeUnidadeMedidaRepository(_db);

    public IMovimentoStockRepository MovimentosStock => new FakeMovimentoStockRepository(_db);

    public IClienteRepository Clientes => new FakeClienteRepository(_db);

    public IFornecedorRepository Fornecedores => new FakeFornecedorRepository(_db);

    public ICompraRepository Compras => new FakeCompraRepository(_db);

    public IVendaRepository Vendas => new FakeVendaRepository(_db);

    public ISessaoCaixaRepository SessoesCaixa => new FakeSessaoCaixaRepository(_db);

    public IUtilizadorRepository Utilizadores => new FakeUtilizadorRepository(_db);

    public ILogAuditoriaRepository LogsAuditoria => new FakeLogAuditoriaRepository(_db);

    // Sem EF Core por trás, "guardar" já aconteceu no momento de cada
    // Adicionar/mutação in-memory — SaveChangesAsync é apenas um no-op
    // que respeita a assinatura do contrato.
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

file sealed class FakeProdutoRepository(InMemoryDatabase db) : IProdutoRepository
{
    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Produtos.FirstOrDefault(p => p.Id == id));

    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras, CancellationToken ct = default) =>
        Task.FromResult(db.Produtos.FirstOrDefault(p =>
            p.CodigoBarras == codigoBarras || p.Apresentacoes.Any(a => a.CodigoBarras == codigoBarras)));

    public Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno, CancellationToken ct = default) =>
        Task.FromResult(db.Produtos.FirstOrDefault(p => p.CodigoInterno == codigoInterno));

    public Task<IReadOnlyList<Produto>> ListarAsync(string? termoPesquisa = null, Guid? categoriaId = null, bool apenasAtivos = true, CancellationToken ct = default)
    {
        var query = db.Produtos.AsEnumerable();

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
            query = query.Where(p => p.Nome.Contains(termoPesquisa, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult<IReadOnlyList<Produto>>(query.ToList());
    }

    public Task AdicionarAsync(Produto produto, CancellationToken ct = default)
    {
        db.Produtos.Add(produto);
        return Task.CompletedTask;
    }
}

file sealed class FakeCategoriaRepository(InMemoryDatabase db) : ICategoriaRepository
{
    public Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Categorias.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Categoria>>(db.Categorias.ToList());

    public Task AdicionarAsync(Categoria categoria, CancellationToken ct = default)
    {
        db.Categorias.Add(categoria);
        return Task.CompletedTask;
    }
}

file sealed class FakeUnidadeMedidaRepository(InMemoryDatabase db) : IUnidadeMedidaRepository
{
    public Task<UnidadeMedida?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.UnidadesMedida.FirstOrDefault(u => u.Id == id));

    public Task<UnidadeMedida?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        Task.FromResult(db.UnidadesMedida.FirstOrDefault(u => u.Codigo == codigo));

    public Task<IReadOnlyList<UnidadeMedida>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<UnidadeMedida>>(db.UnidadesMedida.ToList());

    public Task AdicionarAsync(UnidadeMedida unidadeMedida, CancellationToken ct = default)
    {
        db.UnidadesMedida.Add(unidadeMedida);
        return Task.CompletedTask;
    }
}

file sealed class FakeMovimentoStockRepository(InMemoryDatabase db) : IMovimentoStockRepository
{
    public Task AdicionarAsync(MovimentoStock movimento, CancellationToken ct = default)
    {
        db.MovimentosStock.Add(movimento);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<MovimentoStock>> ListarPorProdutoAsync(Guid produtoId, DateTime? de = null, DateTime? ate = null, int pagina = 1, int tamanhoPagina = 50, CancellationToken ct = default)
    {
        var query = db.MovimentosStock.Where(m => m.ProdutoId == produtoId);

        if (de.HasValue)
        {
            query = query.Where(m => m.Data >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(m => m.Data <= ate.Value);
        }

        var pagina1Indexada = Math.Max(0, pagina - 1);

        return Task.FromResult<IReadOnlyList<MovimentoStock>>(
            query.OrderByDescending(m => m.Data).Skip(pagina1Indexada * tamanhoPagina).Take(tamanhoPagina).ToList());
    }

    public Task<IReadOnlyList<MovimentoStock>> ListarTodosPorProdutoAsync(Guid produtoId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<MovimentoStock>>(db.MovimentosStock.Where(m => m.ProdutoId == produtoId).OrderBy(m => m.Data).ToList());
}

file sealed class FakeClienteRepository(InMemoryDatabase db) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Clientes.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Cliente>> ListarAsync(string? termoPesquisa = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Cliente>>(db.Clientes.ToList());

    public Task AdicionarAsync(Cliente cliente, CancellationToken ct = default)
    {
        db.Clientes.Add(cliente);
        return Task.CompletedTask;
    }
}

file sealed class FakeFornecedorRepository(InMemoryDatabase db) : IFornecedorRepository
{
    public Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Fornecedores.FirstOrDefault(f => f.Id == id));

    public Task<IReadOnlyList<Fornecedor>> ListarAsync(string? termoPesquisa = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Fornecedor>>(db.Fornecedores.ToList());

    public Task AdicionarAsync(Fornecedor fornecedor, CancellationToken ct = default)
    {
        db.Fornecedores.Add(fornecedor);
        return Task.CompletedTask;
    }
}

file sealed class FakeCompraRepository(InMemoryDatabase db) : ICompraRepository
{
    public Task<Compra?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Compras.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Compra>> ListarAsync(Guid? fornecedorId = null, DateTime? de = null, DateTime? ate = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Compra>>(db.Compras.Where(c => !fornecedorId.HasValue || c.FornecedorId == fornecedorId.Value).ToList());

    public Task AdicionarAsync(Compra compra, CancellationToken ct = default)
    {
        db.Compras.Add(compra);
        return Task.CompletedTask;
    }
}

file sealed class FakeVendaRepository(InMemoryDatabase db) : IVendaRepository
{
    public Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Vendas.FirstOrDefault(v => v.Id == id));

    public Task<IReadOnlyList<Venda>> ListarAsync(Guid? utilizadorId = null, Guid? clienteId = null, DateTime? de = null, DateTime? ate = null, CancellationToken ct = default)
    {
        var query = db.Vendas.AsEnumerable();

        if (utilizadorId.HasValue)
        {
            query = query.Where(v => v.UtilizadorId == utilizadorId.Value);
        }

        if (clienteId.HasValue)
        {
            query = query.Where(v => v.ClienteId == clienteId.Value);
        }

        if (de.HasValue)
        {
            query = query.Where(v => v.Data >= de.Value);
        }

        if (ate.HasValue)
        {
            query = query.Where(v => v.Data <= ate.Value);
        }

        return Task.FromResult<IReadOnlyList<Venda>>(query.ToList());
    }

    public Task AdicionarAsync(Venda venda, CancellationToken ct = default)
    {
        db.Vendas.Add(venda);
        return Task.CompletedTask;
    }
}

file sealed class FakeSessaoCaixaRepository(InMemoryDatabase db) : ISessaoCaixaRepository
{
    public Task<SessaoCaixaEntity?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.SessoesCaixa.FirstOrDefault(s => s.Id == id));

    public Task<SessaoCaixaEntity?> ObterAbertaAsync(CancellationToken ct = default) =>
        Task.FromResult(db.SessoesCaixa.FirstOrDefault(s => s.Estado == KiVenda.Core.Enums.EstadoSessaoCaixa.Aberta));

    public Task<IReadOnlyList<SessaoCaixaEntity>> ListarAsync(DateTime? de = null, DateTime? ate = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SessaoCaixaEntity>>(db.SessoesCaixa.ToList());

    public Task AdicionarAsync(SessaoCaixaEntity sessaoCaixa, CancellationToken ct = default)
    {
        db.SessoesCaixa.Add(sessaoCaixa);
        return Task.CompletedTask;
    }
}

file sealed class FakeUtilizadorRepository(InMemoryDatabase db) : IUtilizadorRepository
{
    public Task<Utilizador?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(db.Utilizadores.FirstOrDefault(u => u.Id == id));

    public Task<Utilizador?> ObterPorNomeUtilizadorAsync(string nomeUtilizador, CancellationToken ct = default) =>
        Task.FromResult(db.Utilizadores.FirstOrDefault(u => u.NomeUtilizador == nomeUtilizador));

    public Task<IReadOnlyList<Utilizador>> ListarAsync(bool apenasAtivos = true, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Utilizador>>(db.Utilizadores.Where(u => !apenasAtivos || u.Ativo).ToList());

    public Task AdicionarAsync(Utilizador utilizador, CancellationToken ct = default)
    {
        db.Utilizadores.Add(utilizador);
        return Task.CompletedTask;
    }
}

file sealed class FakeLogAuditoriaRepository(InMemoryDatabase db) : ILogAuditoriaRepository
{
    public Task AdicionarAsync(LogAuditoria log, CancellationToken ct = default)
    {
        db.LogsAuditoria.Add(log);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LogAuditoria>> ListarAsync(Guid? utilizadorId = null, string? entidadeAfetada = null, DateTime? de = null, DateTime? ate = null, int pagina = 1, int tamanhoPagina = 50, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<LogAuditoria>>(db.LogsAuditoria.ToList());
}
