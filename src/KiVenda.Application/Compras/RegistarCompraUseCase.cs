using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Compras;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Compras;

public sealed record ItemCompraCommand(Guid ProdutoId, Guid ApresentacaoId, decimal QuantidadeNaApresentacao, decimal CustoTotalItem);

public sealed record RegistarCompraCommand(Guid FornecedorId, IReadOnlyList<ItemCompraCommand> Itens);

/// <summary>
/// Orquestra, numa única transação: cria a <see cref="Compra"/>, dá
/// entrada de stock em cada produto envolvido (via
/// <see cref="Core.Produtos.Produto.RegistarEntradaStock"/>) e persiste
/// tudo com um único <see cref="IUnitOfWork.SaveChangesAsync"/>.
///
/// Não delega em <see cref="Application.Stock.RegistarEntradaStockUseCase"/>
/// de propósito: esse caso de uso já faz a sua própria verificação de
/// permissão (<see cref="Acao.AjustarStock"/>), e chamá-lo a partir daqui
/// duplicaria essa verificação para uma ação que já foi autorizada pela
/// verificação de <see cref="Acao.RegistarCompras"/> abaixo — ver Fase 3
/// do plano de implementação.
/// </summary>
public sealed class RegistarCompraUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarCompraCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.RegistarCompras);

        if (comando.Itens.Count == 0)
        {
            throw new DomainException("Uma compra tem de ter pelo menos um item.");
        }

        _ = await uow.Fornecedores.ObterPorIdAsync(comando.FornecedorId, cancellationToken)
            ?? throw new DomainException("Fornecedor não encontrado.");

        var compra = new Compra(comando.FornecedorId, contexto.UtilizadorId);

        foreach (var itemComando in comando.Itens)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(itemComando.ProdutoId, cancellationToken)
                ?? throw new DomainException("Produto não encontrado.");

            var itemCompra = compra.AdicionarItem(produto, itemComando.ApresentacaoId, itemComando.QuantidadeNaApresentacao, itemComando.CustoTotalItem);

            var movimento = produto.RegistarEntradaStock(
                itemCompra.QuantidadeUnidadeBase,
                itemCompra.CustoTotalItem,
                OrigemMovimentoStock.Compra,
                compra.Id,
                contexto.UtilizadorId);

            await uow.MovimentosStock.AdicionarAsync(movimento, cancellationToken);
        }

        await uow.Compras.AdicionarAsync(compra, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return compra.Id;
    }
}
