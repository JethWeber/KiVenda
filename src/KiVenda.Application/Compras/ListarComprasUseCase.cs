using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Compras;

public sealed record ListarComprasQuery(Guid? FornecedorId = null, DateTime? De = null, DateTime? Ate = null);

public sealed record ItemCompraDto(Guid ProdutoId, Guid ApresentacaoProdutoId, decimal QuantidadeNaApresentacao, decimal QuantidadeUnidadeBase, decimal CustoTotalItem, decimal CustoUnitarioUnidadeBase);

public sealed record CompraDto(Guid Id, Guid FornecedorId, Guid UtilizadorId, DateTime Data, decimal CustoTotal, IReadOnlyList<ItemCompraDto> Itens);

public sealed class ListarComprasUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<IReadOnlyList<CompraDto>> ExecutarAsync(ListarComprasQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.RegistarCompras);

        var compras = await uow.Compras.ListarAsync(query.FornecedorId, query.De, query.Ate, cancellationToken);

        return compras
            .Select(c => new CompraDto(
                c.Id,
                c.FornecedorId,
                c.UtilizadorId,
                c.Data,
                c.CustoTotal,
                c.Itens
                    .Select(i => new ItemCompraDto(i.ProdutoId, i.ApresentacaoProdutoId, i.QuantidadeNaApresentacao, i.QuantidadeUnidadeBase, i.CustoTotalItem, i.CustoUnitarioUnidadeBase))
                    .ToList()))
            .ToList();
    }
}
