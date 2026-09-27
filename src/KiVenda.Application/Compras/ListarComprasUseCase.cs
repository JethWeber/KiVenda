using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Compras;

public sealed record ListarComprasQuery(Guid? FornecedorId = null, DateTime? De = null, DateTime? Ate = null);

public sealed record ItemCompraDto(
    Guid ProdutoId,
    Guid ApresentacaoProdutoId,
    decimal QuantidadeNaApresentacao,
    decimal QuantidadeUnidadeBase,
    decimal CustoTotalItem,
    decimal CustoUnitarioUnidadeBase,
    string? ProdutoNome = null,
    string? ApresentacaoNome = null);

public sealed record CompraDto(
    Guid Id,
    Guid FornecedorId,
    Guid UtilizadorId,
    DateTime Data,
    decimal CustoTotal,
    IReadOnlyList<ItemCompraDto> Itens,
    string? FornecedorNome = null);

public sealed class ListarComprasUseCase(
    IUnitOfWork uow,
    IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<CompraDto>> ExecutarAsync(
        ListarComprasQuery query,
        CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.RegistarCompras);

        var compras = await uow.Compras.ListarAsync(
            query.FornecedorId,
            query.De,
            query.Ate,
            cancellationToken);

        var produtoIds = compras
            .SelectMany(c => c.Itens)
            .Select(i => i.ProdutoId)
            .Distinct()
            .ToList();

        var fornecedorIds = compras
            .Select(c => c.FornecedorId)
            .Distinct()
            .ToList();

        var produtos = new Dictionary<Guid, KiVenda.Core.Produtos.Produto>();
        foreach (var id in produtoIds)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(id, cancellationToken);
            if (produto is not null)
                produtos[id] = produto;
        }

        var fornecedores = new Dictionary<Guid, KiVenda.Core.Fornecedores.Fornecedor>();
        foreach (var id in fornecedorIds)
        {
            var fornecedor = await uow.Fornecedores.ObterPorIdAsync(id, cancellationToken);
            if (fornecedor is not null)
                fornecedores[id] = fornecedor;
        }

        return compras
            .Select(c => new CompraDto(
                c.Id,
                c.FornecedorId,
                c.UtilizadorId,
                c.Data,
                c.CustoTotal,
                c.Itens
                    .Select(i =>
                    {
                        produtos.TryGetValue(i.ProdutoId, out var produto);
                        var apresentacao = produto?.Apresentacoes
                            .FirstOrDefault(a => a.Id == i.ApresentacaoProdutoId);

                        return new ItemCompraDto(
                            i.ProdutoId,
                            i.ApresentacaoProdutoId,
                            i.QuantidadeNaApresentacao,
                            i.QuantidadeUnidadeBase,
                            i.CustoTotalItem,
                            i.CustoUnitarioUnidadeBase,
                            produto?.Nome,
                            apresentacao?.Nome);
                    })
                    .ToList(),
                fornecedores.TryGetValue(c.FornecedorId, out var fornecedor)
                    ? fornecedor.Nome
                    : null))
            .ToList();
    }
}
