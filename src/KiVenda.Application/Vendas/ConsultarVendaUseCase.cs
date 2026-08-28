using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Vendas;

public sealed record ConsultarVendaQuery(Guid VendaId);

public sealed record ItemVendaDto(
    Guid Id,
    Guid ProdutoId,
    string ProdutoNome,
    Guid ApresentacaoId,
    string ApresentacaoNome,
    decimal QuantidadeNaApresentacao,
    decimal ValorTotal);

public sealed record VendaDto(
    Guid Id,
    Guid? ClienteId,
    decimal Subtotal,
    decimal Desconto,
    decimal Total,
    EstadoVenda Estado,
    IReadOnlyList<ItemVendaDto> Itens);

/// <summary>
/// Consulta o estado atual de uma venda (Fase 7: usado pelo PDV para
/// desenhar o carrinho depois de cada AdicionarItem/RemoverItem/
/// AplicarDesconto — nenhum desses casos de uso devolve a venda
/// completa, só confirma a operação).
/// </summary>
public sealed class ConsultarVendaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<VendaDto> ExecutarAsync(ConsultarVendaQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.FazerVenda);

        var venda = await uow.Vendas.ObterPorIdAsync(query.VendaId, cancellationToken)
            ?? throw new DomainException("Venda não encontrada.");

        var itens = new List<ItemVendaDto>();
        foreach (var item in venda.Itens)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(item.ProdutoId, cancellationToken);
            var apresentacao = produto?.Apresentacoes.FirstOrDefault(a => a.Id == item.ApresentacaoProdutoId);

            itens.Add(new ItemVendaDto(
                item.Id,
                item.ProdutoId,
                produto?.Nome ?? "(produto removido)",
                item.ApresentacaoProdutoId,
                apresentacao?.Nome ?? "—",
                item.QuantidadeNaApresentacao,
                item.ValorTotal));
        }

        return new VendaDto(venda.Id, venda.ClienteId, venda.Subtotal, venda.Desconto, venda.Total, venda.Estado, itens);
    }
}
