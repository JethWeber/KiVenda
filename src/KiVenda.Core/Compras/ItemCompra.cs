using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;

namespace KiVenda.Core.Compras;

/// <summary>
/// Item de uma <see cref="Compra"/>. Guarda tanto a quantidade na
/// apresentação efetivamente comprada (ex.: "1 saco de 25 kg") como a
/// quantidade já convertida para a unidade base do produto (ex.: 25000 g),
/// para que o custo por unidade base possa ser calculado sem ambiguidade.
/// </summary>
public sealed class ItemCompra : Entity
{
    public Guid ProdutoId { get; private set; }

    public Guid ApresentacaoProdutoId { get; private set; }

    public decimal QuantidadeNaApresentacao { get; private set; }

    public decimal QuantidadeUnidadeBase { get; private set; }

    public decimal CustoTotalItem { get; private set; }

    /// <summary>Custo por unidade base derivado deste item (ex.: 27.500 Kz / 25.000 g = 1,10 Kz/g).</summary>
    public decimal CustoUnitarioUnidadeBase => QuantidadeUnidadeBase == 0 ? 0 : CustoTotalItem / QuantidadeUnidadeBase;

    private ItemCompra()
    {
    }

    /// <summary>
    /// Cria um item de compra a partir da apresentação efetivamente
    /// comprada, convertendo a quantidade para a unidade base do produto.
    /// </summary>
    internal ItemCompra(Produto produto, ApresentacaoProduto apresentacao, decimal quantidadeNaApresentacao, decimal custoTotalItem)
    {
        if (apresentacao.ProdutoId != produto.Id)
        {
            throw new DomainException("A apresentação indicada não pertence a este produto.");
        }

        if (custoTotalItem < 0)
        {
            throw new DomainException("O custo total do item de compra não pode ser negativo.");
        }

        ProdutoId = produto.Id;
        ApresentacaoProdutoId = apresentacao.Id;
        QuantidadeNaApresentacao = quantidadeNaApresentacao;
        QuantidadeUnidadeBase = apresentacao.ConverterParaUnidadeBase(quantidadeNaApresentacao);
        CustoTotalItem = custoTotalItem;
    }
}
