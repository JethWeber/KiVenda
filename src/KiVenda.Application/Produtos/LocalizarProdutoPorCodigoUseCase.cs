using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Produtos;

/// <summary>
/// Origem pela qual o código foi resolvido — útil para o PDV saber se
/// o bip veio de um EAN de apresentação específica (ex.: saco de 1 kg)
/// ou do código genérico do produto / código interno.
/// </summary>
public enum OrigemCodigoResolvido
{
    CodigoBarrasProduto,
    CodigoBarrasApresentacao,
    CodigoInterno
}

/// <summary>
/// Resultado de <see cref="LocalizarProdutoPorCodigoUseCase"/>.
/// Inclui o produto completo (para a UI preencher o carrinho) e a
/// apresentação concreta que o código bipado/digitado resolveu.
/// </summary>
public sealed record ProdutoLocalizadoDto(
    ProdutoDto Produto,
    Guid ApresentacaoId,
    string NomeApresentacao,
    OrigemCodigoResolvido Origem);

public sealed record LocalizarProdutoPorCodigoQuery(string Codigo);

/// <summary>
/// Ponto único de "dado um código, qual produto/apresentação é este".
/// Ordem de resolução (conforme plano Fase 8, Parte 2):
/// <list type="number">
///   <item><see cref="IProdutoRepository.ObterPorCodigoBarrasAsync"/> — cobre
///     o código de barras do produto e o de qualquer apresentação;</item>
///   <item><see cref="IProdutoRepository.ObterPorCodigoInternoAsync"/> — fallback
///     para produtos sem código de barras.</item>
/// </list>
/// Devolve <c>null</c> quando nada corresponde (em vez de lançar), para o
/// PDV poder mostrar um aviso amigável sem tratar excepção como erro.
/// </summary>
public sealed class LocalizarProdutoPorCodigoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<ProdutoLocalizadoDto?> ExecutarAsync(
        LocalizarProdutoPorCodigoQuery query,
        CancellationToken cancellationToken = default)
    {
        // Tanto Gerente como Atendente podem localizar (é pré-requisito do PDV).
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        if (string.IsNullOrWhiteSpace(query.Codigo))
        {
            return null;
        }

        var codigo = query.Codigo.Trim();

        var produto = await uow.Produtos.ObterPorCodigoBarrasAsync(codigo, cancellationToken);
        if (produto is not null && produto.Ativo)
        {
            return ResolverPorCodigoBarras(produto, codigo);
        }

        produto = await uow.Produtos.ObterPorCodigoInternoAsync(codigo, cancellationToken);
        if (produto is not null && produto.Ativo)
        {
            var apresentacaoPadrao = ResolverApresentacaoPadrao(produto);
            return new ProdutoLocalizadoDto(
                Mapear(produto),
                apresentacaoPadrao.Id,
                apresentacaoPadrao.Nome,
                OrigemCodigoResolvido.CodigoInterno);
        }

        return null;
    }

    private static ProdutoLocalizadoDto ResolverPorCodigoBarras(Produto produto, string codigo)
    {
        // Preferência: apresentação com o código de barras exacto bipado.
        var apresentacaoPorCodigo = produto.Apresentacoes
            .FirstOrDefault(a => a.Ativa
                && a.CodigoBarras is not null
                && string.Equals(a.CodigoBarras, codigo, StringComparison.Ordinal));

        if (apresentacaoPorCodigo is not null)
        {
            return new ProdutoLocalizadoDto(
                Mapear(produto),
                apresentacaoPorCodigo.Id,
                apresentacaoPorCodigo.Nome,
                OrigemCodigoResolvido.CodigoBarrasApresentacao);
        }

        // Código de barras do próprio produto → apresentação padrão (unidade base).
        var apresentacaoPadrao = ResolverApresentacaoPadrao(produto);
        return new ProdutoLocalizadoDto(
            Mapear(produto),
            apresentacaoPadrao.Id,
            apresentacaoPadrao.Nome,
            OrigemCodigoResolvido.CodigoBarrasProduto);
    }

    /// <summary>
    /// Apresentação "unidade base" (fator 1) activa; se por algum motivo
    /// não existir, cai na primeira activa do produto.
    /// </summary>
    private static ApresentacaoProduto ResolverApresentacaoPadrao(Produto produto)
    {
        var padrao = produto.Apresentacoes
            .FirstOrDefault(a => a.Ativa && a.FatorConversaoParaUnidadeBase == 1m)
            ?? produto.Apresentacoes.FirstOrDefault(a => a.Ativa)
            ?? produto.Apresentacoes.First();

        return padrao;
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
                .Select(a => new ApresentacaoProdutoDto(
                    a.Id, a.Nome, a.FatorConversaoParaUnidadeBase, a.CodigoBarras, a.Ativa))
                .ToList());
    }
}
