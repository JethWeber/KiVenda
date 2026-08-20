using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

public sealed record ListarProdutosQuery(
    string? TermoPesquisa = null,
    Guid? CategoriaId = null,
    bool ApenasAtivos = true);

public sealed class ListarProdutosUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<ProdutoDto>> ExecutarAsync(ListarProdutosQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var produtos = await uow.Produtos.ListarAsync(query.TermoPesquisa, query.CategoriaId, query.ApenasAtivos, cancellationToken);

        return produtos.Select(Mapear).ToList();
    }

    private static ProdutoDto Mapear(Produto produto)
    {
        return new ProdutoDto(
            produto.Id,
            produto.Nome,
            produto.CodigoInterno,
            produto.CodigoBarras,
            produto.CategoriaId,
            produto.UnidadeBaseId,
            produto.PrecoVendaPorUnidadeBase,
            produto.StockMinimo,
            produto.EstoqueAtual,
            produto.CustoMedioPonderado,
            produto.CalcularValorEstoque(),
            produto.ObterEstadoStock(),
            produto.Ativo,
            produto.Apresentacoes
                .Select(a => new ApresentacaoProdutoDto(a.Id, a.Nome, a.FatorConversaoParaUnidadeBase, a.CodigoBarras, a.Ativa))
                .ToList());
    }
}
