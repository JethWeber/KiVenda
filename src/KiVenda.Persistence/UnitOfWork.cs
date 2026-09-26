using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Persistence.Repositories;

namespace KiVenda.Persistence;

public sealed class UnitOfWork(KiVendaDbContext context) : IUnitOfWork
{
    private IProdutoRepository? _produtos;
    private ICategoriaRepository? _categorias;
    private IUnidadeMedidaRepository? _unidadesMedida;
    private IMovimentoStockRepository? _movimentosStock;
    private IClienteRepository? _clientes;
    private IFornecedorRepository? _fornecedores;
    private IFuncionarioRepository? _funcionarios;
    private IEmpresaRepository? _empresas;
    private ICompraRepository? _compras;
    private IVendaRepository? _vendas;
    private ISessaoCaixaRepository? _sessoesCaixa;
    private IUtilizadorRepository? _utilizadores;
    private ILogAuditoriaRepository? _logsAuditoria;
    private INotificacaoRepository? _notificacoes;

    public IProdutoRepository Produtos => _produtos ??= new ProdutoRepository(context);
    public ICategoriaRepository Categorias => _categorias ??= new CategoriaRepository(context);
    public IUnidadeMedidaRepository UnidadesMedida => _unidadesMedida ??= new UnidadeMedidaRepository(context);
    public IMovimentoStockRepository MovimentosStock => _movimentosStock ??= new MovimentoStockRepository(context);
    public IClienteRepository Clientes => _clientes ??= new ClienteRepository(context);
    public IFornecedorRepository Fornecedores => _fornecedores ??= new FornecedorRepository(context);
    public IFuncionarioRepository Funcionarios => _funcionarios ??= new FuncionarioRepository(context);
    public IEmpresaRepository Empresas => _empresas ??= new EmpresaRepository(context);
    public ICompraRepository Compras => _compras ??= new CompraRepository(context);
    public IVendaRepository Vendas => _vendas ??= new VendaRepository(context);
    public ISessaoCaixaRepository SessoesCaixa => _sessoesCaixa ??= new SessaoCaixaRepository(context);
    public IUtilizadorRepository Utilizadores => _utilizadores ??= new UtilizadorRepository(context);
    public ILogAuditoriaRepository LogsAuditoria => _logsAuditoria ??= new LogAuditoriaRepository(context);
    public INotificacaoRepository Notificacoes => _notificacoes ??= new NotificacaoRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => context.SaveChangesAsync(cancellationToken);
    public ValueTask DisposeAsync() => context.DisposeAsync();
}