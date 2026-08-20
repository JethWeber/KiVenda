using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

public sealed record ConsultarMovimentosStockQuery(
    Guid ProdutoId,
    DateTime? De = null,
    DateTime? Ate = null,
    int Pagina = 1,
    int TamanhoPagina = 50);

public sealed record MovimentoStockDto(
    Guid Id,
    TipoMovimentoStock Tipo,
    decimal Quantidade,
    decimal? CustoUnitarioUnidadeBase,
    OrigemMovimentoStock Origem,
    Guid? OrigemId,
    Guid UtilizadorId,
    string? Motivo,
    DateTime Data);

/// <summary>
/// Histórico paginado de movimentos de um produto — usado tanto pela UI
/// (aba "Movimentos de Stock" no cadastro de Produtos, Fase 6.3) como
/// para investigar divergências de caixa/stock.
/// </summary>
public sealed class ConsultarMovimentosStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<MovimentoStockDto>> ExecutarAsync(ConsultarMovimentosStockQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var movimentos = await uow.MovimentosStock.ListarPorProdutoAsync(
            query.ProdutoId, query.De, query.Ate, query.Pagina, query.TamanhoPagina, cancellationToken);

        return movimentos
            .Select(m => new MovimentoStockDto(m.Id, m.Tipo, m.Quantidade, m.CustoUnitarioUnidadeBase, m.Origem, m.OrigemId, m.UtilizadorId, m.Motivo, m.Data))
            .ToList();
    }
}
