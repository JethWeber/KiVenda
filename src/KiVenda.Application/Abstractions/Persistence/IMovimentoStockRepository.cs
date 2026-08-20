using KiVenda.Core.Produtos;

namespace KiVenda.Application.Abstractions.Persistence;

/// <summary>
/// Contrato de acesso ao histórico de <see cref="MovimentoStock"/> —
/// fonte de verdade do estoque. Consultado isoladamente do agregado
/// <see cref="Produto"/> porque o histórico pode ser extenso e é
/// paginado (usado por ConsultarMovimentosStock e
/// RecalcularEstoqueMaterializado — Fase 3).
/// </summary>
public interface IMovimentoStockRepository
{
    Task AdicionarAsync(MovimentoStock movimento, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovimentoStock>> ListarPorProdutoAsync(
        Guid produtoId,
        DateTime? de = null,
        DateTime? ate = null,
        int pagina = 1,
        int tamanhoPagina = 50,
        CancellationToken cancellationToken = default);

    /// <summary>Histórico completo (sem paginação) de um produto, usado para recalcular o estoque materializado.</summary>
    Task<IReadOnlyList<MovimentoStock>> ListarTodosPorProdutoAsync(Guid produtoId, CancellationToken cancellationToken = default);
}
