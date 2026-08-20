using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Vendas;

public sealed record PagamentoCommand(MetodoPagamento Metodo, decimal Valor);

public sealed record FinalizarVendaCommand(Guid VendaId, IReadOnlyList<PagamentoCommand> Pagamentos);

public sealed record ItemReciboDto(string ProdutoNome, string ApresentacaoNome, decimal QuantidadeNaApresentacao, decimal ValorTotal);

/// <summary>
/// Resultado devolvido ao caller (UI/Infrastructure), com o suficiente
/// para emitir o recibo (Fase 4) sem precisar de reconsultar a venda.
/// </summary>
public sealed record ReciboVendaDto(
    Guid VendaId,
    DateTime Data,
    IReadOnlyList<ItemReciboDto> Itens,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    decimal LucroEstimado,
    IReadOnlyList<PagamentoCommand> Pagamentos);

/// <summary>
/// Módulo central do sistema. Numa única transação (um único
/// <see cref="IUnitOfWork.SaveChangesAsync"/> no final):
///   1. Regista os pagamentos e finaliza a venda (valida no Core que o
///      total pago cobre o total da venda).
///   2. Dá saída de stock em cada produto vendido, na unidade base
///      convertida a partir da apresentação vendida.
///   3. Regista a entrada correspondente na sessão de caixa aberta.
///   4. Regista o evento em auditoria ("Venda realizada").
///
/// Não delega em <see cref="Application.Stock.RegistarSaidaStockUseCase"/>
/// pela mesma razão documentada em <see cref="Compras.RegistarCompraUseCase"/>:
/// evitar uma segunda verificação de permissão aninhada.
/// </summary>
public sealed class FinalizarVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<ReciboVendaDto> ExecutarAsync(FinalizarVendaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(comando.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        foreach (var pagamento in comando.Pagamentos)
        {
            venda.AdicionarPagamento(pagamento.Metodo, pagamento.Valor);
        }

        venda.Finalizar();

        var itensRecibo = new List<ItemReciboDto>();

        foreach (var item in venda.Itens)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(item.ProdutoId, cancellationToken)
                ?? throw new DomainException("Produto não encontrado ao dar saída de stock.");

            var apresentacao = produto.ObterApresentacao(item.ApresentacaoProdutoId);

            var movimento = produto.RegistarSaidaStock(
                item.QuantidadeUnidadeBase,
                OrigemMovimentoStock.Venda,
                venda.Id,
                contexto.UtilizadorId);

            await uow.MovimentosStock.AdicionarAsync(movimento, cancellationToken);

            itensRecibo.Add(new ItemReciboDto(produto.Nome, apresentacao.Nome, item.QuantidadeNaApresentacao, item.ValorTotal));
        }

        var sessaoCaixa = await uow.SessoesCaixa.ObterPorIdAsync(venda.SessaoCaixaId, cancellationToken)
            ?? throw new DomainException("Sessão de caixa da venda não encontrada.");

        sessaoCaixa.RegistarEntradaDeVenda(venda.Total, contexto.UtilizadorId, venda.Id);

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(contexto.UtilizadorId, "Venda realizada", "Venda", venda.Id, dadosDepois: venda.Total.ToString("0.00")),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return new ReciboVendaDto(
            venda.Id,
            venda.Data,
            itensRecibo,
            venda.Subtotal,
            venda.Desconto,
            venda.Total,
            venda.LucroEstimado,
            comando.Pagamentos);
    }
}
