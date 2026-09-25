namespace KiVenda.Application.Abstractions.Persistence;

/// <summary>Unidade de trabalho sobre a base local SQLite.</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IProdutoRepository Produtos { get; }
    ICategoriaRepository Categorias { get; }
    IUnidadeMedidaRepository UnidadesMedida { get; }
    IMovimentoStockRepository MovimentosStock { get; }
    IClienteRepository Clientes { get; }
    IEmpresaRepository Empresas { get; }
    IFornecedorRepository Fornecedores { get; }
    ICompraRepository Compras { get; }
    IVendaRepository Vendas { get; }
    ISessaoCaixaRepository SessoesCaixa { get; }
    IUtilizadorRepository Utilizadores { get; }
    ILogAuditoriaRepository LogsAuditoria { get; }
    INotificacaoRepository Notificacoes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}