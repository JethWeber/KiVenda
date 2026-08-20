using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;

namespace KiVenda.Core.Vendas;

/// <summary>
/// Item de uma <see cref="Venda"/>. O preço e o custo por unidade base
/// são "fotografados" (snapshot) do <see cref="Produto"/> no momento em
/// que o item é adicionado, para que alterações posteriores de preço ou
/// de custo médio ponderado não distorçam vendas já registadas.
/// </summary>
public sealed class ItemVenda : Entity
{
    public Guid ProdutoId { get; private set; }

    public Guid ApresentacaoProdutoId { get; private set; }

    public decimal QuantidadeNaApresentacao { get; private set; }

    public decimal QuantidadeUnidadeBase { get; private set; }

    /// <summary>Preço de venda por unidade base, fotografado no momento da venda.</summary>
    public decimal PrecoUnitarioUnidadeBase { get; private set; }

    /// <summary>Custo médio ponderado por unidade base, fotografado no momento da venda (base do cálculo de lucro).</summary>
    public decimal CustoUnitarioUnidadeBase { get; private set; }

    public decimal ValorTotal => PrecoUnitarioUnidadeBase * QuantidadeUnidadeBase;

    public decimal LucroEstimado => (PrecoUnitarioUnidadeBase - CustoUnitarioUnidadeBase) * QuantidadeUnidadeBase;

    private ItemVenda()
    {
    }

    internal ItemVenda(Produto produto, ApresentacaoProduto apresentacao, decimal quantidadeNaApresentacao)
    {
        if (apresentacao.ProdutoId != produto.Id)
        {
            throw new DomainException("A apresentação indicada não pertence a este produto.");
        }

        if (quantidadeNaApresentacao <= 0)
        {
            throw new DomainException("A quantidade vendida tem de ser positiva.");
        }

        ProdutoId = produto.Id;
        ApresentacaoProdutoId = apresentacao.Id;
        QuantidadeNaApresentacao = quantidadeNaApresentacao;
        QuantidadeUnidadeBase = apresentacao.ConverterParaUnidadeBase(quantidadeNaApresentacao);
        PrecoUnitarioUnidadeBase = produto.PrecoVendaPorUnidadeBase;
        CustoUnitarioUnidadeBase = produto.CustoMedioPonderado;
    }
}
