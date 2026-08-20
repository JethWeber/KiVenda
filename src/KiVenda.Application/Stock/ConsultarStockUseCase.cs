using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

public sealed record ConsultarStockQuery(Guid ProdutoId, Guid? ApresentacaoId = null);

public sealed record StockDto(
    Guid ProdutoId,
    decimal EstoqueAtualUnidadeBase,
    decimal? EstoqueAtualNaApresentacao,
    decimal StockMinimo,
    EstadoStock EstadoStock,
    decimal CustoMedioPonderado,
    decimal ValorEstoque);

public sealed class ConsultarStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<StockDto> ExecutarAsync(ConsultarStockQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var produto = await uow.Produtos.ObterPorIdAsync(query.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        decimal? estoqueNaApresentacao = null;
        if (query.ApresentacaoId.HasValue)
        {
            var apresentacao = produto.ObterApresentacao(query.ApresentacaoId.Value);
            estoqueNaApresentacao = apresentacao.ConverterDeUnidadeBase(produto.EstoqueAtual);
        }

        return new StockDto(
            produto.Id,
            produto.EstoqueAtual,
            estoqueNaApresentacao,
            produto.StockMinimo,
            produto.ObterEstadoStock(),
            produto.CustoMedioPonderado,
            produto.CalcularValorEstoque());
    }
}
