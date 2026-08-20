using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Stock;

/// <summary>
/// Existe como caso de uso independente (conforme o plano de
/// implementação), mas o fluxo principal do MVP — a saída por venda —
/// é orquestrado diretamente dentro de FinalizarVenda (módulo Vendas),
/// não através deste caso de uso, pela mesma razão de
/// <see cref="RegistarEntradaStockUseCase"/>: evitar uma segunda
/// verificação de permissão aninhada quando FinalizarVenda (que exige
/// <see cref="Acao.FazerVenda"/>) precisa de dar saída de stock, uma
/// ação que só por si exigiria <see cref="Acao.AjustarStock"/>. Este
/// caso de uso fica disponível para uma eventual saída manual avulsa
/// (não coberta pela UI do MVP).
/// </summary>
public sealed record RegistarSaidaStockCommand(
    Guid ProdutoId,
    Guid ApresentacaoId,
    decimal QuantidadeNaApresentacao);

public sealed class RegistarSaidaStockUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarSaidaStockCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AjustarStock);

        var produto = await uow.Produtos.ObterPorIdAsync(comando.ProdutoId, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var apresentacao = produto.ObterApresentacao(comando.ApresentacaoId);
        var quantidadeUnidadeBase = apresentacao.ConverterParaUnidadeBase(comando.QuantidadeNaApresentacao);

        var movimento = produto.RegistarSaidaStock(
            quantidadeUnidadeBase,
            OrigemMovimentoStock.AjusteManual,
            origemId: null,
            contexto.UtilizadorId);

        await uow.MovimentosStock.AdicionarAsync(movimento, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return movimento.Id;
    }
}
