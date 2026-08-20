using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Persistence.Repositories;

namespace KiVenda.Persistence;

/// <summary>
/// Implementação do <see cref="IUnitOfWork"/> sobre um único
/// <see cref="KiVendaDbContext"/>. Os repositórios são criados de forma
/// lazy (uma única vez, na primeira utilização) e partilham o mesmo
/// DbContext, para que <see cref="SaveChangesAsync"/> persista tudo
/// numa única transação — ex.: FinalizarVenda (Fase 3) faz a saída de
/// stock do produto, a entrada de caixa da sessão e a criação da venda
/// em memória, e só no fim chama isto uma vez.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly KiVendaDbContext _context;

    private IProdutoRepository? _produtos;
    private ICategoriaRepository? _categorias;
    private IUnidadeMedidaRepository? _unidadesMedida;
    private IMovimentoStockRepository? _movimentosStock;
    private IClienteRepository? _clientes;
    private IFornecedorRepository? _fornecedores;
    private ICompraRepository? _compras;
    private IVendaRepository? _vendas;
    private ISessaoCaixaRepository? _sessoesCaixa;
    private IUtilizadorRepository? _utilizadores;
    private ILogAuditoriaRepository? _logsAuditoria;

    public UnitOfWork(KiVendaDbContext context)
    {
        _context = context;
    }

    public IProdutoRepository Produtos => _produtos ??= new ProdutoRepository(_context);

    public ICategoriaRepository Categorias => _categorias ??= new CategoriaRepository(_context);

    public IUnidadeMedidaRepository UnidadesMedida => _unidadesMedida ??= new UnidadeMedidaRepository(_context);

    public IMovimentoStockRepository MovimentosStock => _movimentosStock ??= new MovimentoStockRepository(_context);

    public IClienteRepository Clientes => _clientes ??= new ClienteRepository(_context);

    public IFornecedorRepository Fornecedores => _fornecedores ??= new FornecedorRepository(_context);

    public ICompraRepository Compras => _compras ??= new CompraRepository(_context);

    public IVendaRepository Vendas => _vendas ??= new VendaRepository(_context);

    public ISessaoCaixaRepository SessoesCaixa => _sessoesCaixa ??= new SessaoCaixaRepository(_context);

    public IUtilizadorRepository Utilizadores => _utilizadores ??= new UtilizadorRepository(_context);

    public ILogAuditoriaRepository LogsAuditoria => _logsAuditoria ??= new LogAuditoriaRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
