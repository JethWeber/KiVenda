namespace KiVenda.Application.Abstractions.Persistence;

/// <summary>
/// Unidade de trabalho: agrega todos os repositórios e garante que
/// operações compostas (ex.: venda + saída de stock + movimento de
/// caixa) são persistidas numa única transação através de
/// <see cref="SaveChangesAsync"/>.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IProdutoRepository Produtos { get; }

    ICategoriaRepository Categorias { get; }

    IUnidadeMedidaRepository UnidadesMedida { get; }

    IMovimentoStockRepository MovimentosStock { get; }

    IClienteRepository Clientes { get; }

    IFornecedorRepository Fornecedores { get; }

    ICompraRepository Compras { get; }

    IVendaRepository Vendas { get; }

    ISessaoCaixaRepository SessoesCaixa { get; }

    IUtilizadorRepository Utilizadores { get; }

    ILogAuditoriaRepository LogsAuditoria { get; }

    /// <summary>
    /// Persiste todas as alterações rastreadas desde o início da unidade
    /// de trabalho, numa única transação. Casos de uso da Application
    /// (Fase 3) que envolvem mais do que um agregado (ex.: FinalizarVenda)
    /// devem fazer todas as mutações em memória e chamar isto uma única
    /// vez no final.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
