using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

/// <summary>
/// Forma comercial em que um produto é comprado/vendido (ex.: "250 g",
/// "1 kg", "Caixinha de 50 un"). Converte-se sempre para/de a
/// <see cref="UnidadeMedida"/> base do produto através de
/// <see cref="FatorConversaoParaUnidadeBase"/>.
/// </summary>
/// <example>
/// Açúcar, unidade base = grama:
///   Apresentação "1 kg" tem FatorConversaoParaUnidadeBase = 1000
///   (1 unidade desta apresentação equivale a 1000 g).
/// </example>
public sealed class ApresentacaoProduto : Entity
{
    public Guid ProdutoId { get; private set; }

    public string Nome { get; private set; } = null!;

    /// <summary>
    /// Quantas unidades base equivalem a 1 unidade desta apresentação.
    /// Deve ser sempre positivo. Uma apresentação com fator 1 representa
    /// a própria unidade base (ex.: "Unidade" para produtos vendidos ao
    /// número, "1 g" para produtos vendidos ao peso solto).
    /// </summary>
    public decimal FatorConversaoParaUnidadeBase { get; private set; }

    /// <summary>
    /// Código de barras próprio desta apresentação, quando o produto é
    /// vendido em embalagem fechada com EAN próprio (ex.: o saco de 1 kg
    /// tem um código diferente do saco de 25 kg). Opcional.
    /// </summary>
    public string? CodigoBarras { get; private set; }

    public bool Ativa { get; private set; } = true;

    private ApresentacaoProduto()
    {
    }

    public ApresentacaoProduto(Guid produtoId, string nome, decimal fatorConversaoParaUnidadeBase, string? codigoBarras = null)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("A apresentação tem de estar associada a um produto.");
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome da apresentação é obrigatório.");
        }

        if (fatorConversaoParaUnidadeBase <= 0)
        {
            throw new DomainException("O fator de conversão da apresentação tem de ser positivo.");
        }

        ProdutoId = produtoId;
        Nome = nome.Trim();
        FatorConversaoParaUnidadeBase = fatorConversaoParaUnidadeBase;
        CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();
    }

    /// <summary>
    /// Converte uma quantidade expressa nesta apresentação (ex.: 2 "sacos de 1 kg")
    /// para a unidade base do produto (ex.: 2000 g).
    /// </summary>
    public decimal ConverterParaUnidadeBase(decimal quantidadeNaApresentacao)
    {
        if (quantidadeNaApresentacao <= 0)
        {
            throw new DomainException("A quantidade a converter tem de ser positiva.");
        }

        return quantidadeNaApresentacao * FatorConversaoParaUnidadeBase;
    }

    /// <summary>
    /// Converte uma quantidade expressa na unidade base do produto
    /// (ex.: 2300 g) para esta apresentação (ex.: 2,3 "kg"), útil para
    /// exibir o estoque de forma legível na UI.
    /// </summary>
    public decimal ConverterDeUnidadeBase(decimal quantidadeUnidadeBase)
    {
        return quantidadeUnidadeBase / FatorConversaoParaUnidadeBase;
    }

    public void Renomear(string novoNome)
    {
        if (string.IsNullOrWhiteSpace(novoNome))
        {
            throw new DomainException("O nome da apresentação é obrigatório.");
        }

        Nome = novoNome.Trim();
        MarcarComoAtualizado();
    }

    public void Inativar()
    {
        Ativa = false;
        MarcarComoAtualizado();
    }

    public void Reativar()
    {
        Ativa = true;
        MarcarComoAtualizado();
    }
}
