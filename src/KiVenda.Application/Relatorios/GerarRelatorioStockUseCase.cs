using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Relatorios;

public sealed record ProdutoStockDto(Guid ProdutoId, string ProdutoNome, decimal EstoqueAtual, decimal StockMinimo);

public sealed record RelatorioStockDto(
    IReadOnlyList<ProdutoStockDto> ProdutosEmFalta,
    IReadOnlyList<ProdutoStockDto> ProdutosComStockBaixo);

/// <summary>
/// Relatório essencial de stock — produtos em falta, stock baixo
/// (Secção 4.8). Restrito ao Gerente.
/// </summary>
public sealed class GerarRelatorioStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<RelatorioStockDto> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AcederRelatorios);

        var produtos = await uow.Produtos.ListarAsync(apenasAtivos: true, cancellationToken: cancellationToken);

        var emFalta = new List<ProdutoStockDto>();
        var stockBaixo = new List<ProdutoStockDto>();

        foreach (var produto in produtos)
        {
            var dto = new ProdutoStockDto(produto.Id, produto.Nome, produto.EstoqueAtual, produto.StockMinimo);

            switch (produto.ObterEstadoStock())
            {
                case EstadoStock.SemStock:
                    emFalta.Add(dto);
                    break;
                case EstadoStock.StockBaixo:
                    stockBaixo.Add(dto);
                    break;
            }
        }

        return new RelatorioStockDto(emFalta, stockBaixo);
    }
}
