namespace KiVenda.Application.Abstractions.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    IProdutoRepository Produtos { get; }
    ICategoriaRepository Categorias { get; }
    IUnidadeMedidaRepository UnidadesMedida { get; }
    IMovimentoStockRepository MovimentosStock { get; }
    IClienteRepository Clientes { get; }
    IFornecedorRepository Fornecedores { get; }
    IFuncionarioRepository Funcionarios { get; }
    IEmpresaRepository Empresas { get; }
    ICompraRepository Compras { get; }
    IVendaRepository Vendas { get; }
    ISessaoCaixaRepository SessoesCaixa { get; }
    IUtilizadorRepository Utilizadores { get; }
    ILogAuditoriaRepository LogsAuditoria { get; }
    INotificacaoRepository Notificacoes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}