using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

/// <summary>
/// Usado diretamente para entradas avulsas (botão "Entrada de Stock" no
/// cadastro de Produtos — Fase 6.3), quando não há um registo formal de
/// <see cref="Core.Compras.Compra"/> associado. Quando a entrada vem de
/// uma compra, é o caso de uso RegistarCompra (módulo Compras) que
/// orquestra a entrada de stock diretamente — não este caso de uso — para
/// evitar uma segunda verificação de permissão aninhada (ver nota na
/// Fase 3 do plano de implementação).
/// </summary>
public sealed record RegistarEntradaStockCommand(
    Guid ProdutoId,
    Guid ApresentacaoId,
    decimal QuantidadeNaApresentacao,
    decimal CustoTotal,
    Guid? LoteId = null);

public sealed class RegistarEntradaStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarEntradaStockCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AjustarStock);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var apresentacao = produto.ObterApresentacao(comando.ApresentacaoId);
        var quantidadeUnidadeBase = apresentacao.ConverterParaUnidadeBase(comando.QuantidadeNaApresentacao);

        var movimento = produto.RegistarEntradaStock(
            quantidadeUnidadeBase,
            comando.CustoTotal,
            OrigemMovimentoStock.Compra,
            origemId: null, // entrada avulsa, sem Compra formal associada
            contexto.UtilizadorId,
            comando.LoteId);

        await uow.MovimentosStock.AdicionarAsync(movimento, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return movimento.Id;
    }
}
