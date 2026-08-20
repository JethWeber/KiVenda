namespace KiVenda.Core.Enums;

/// <summary>
/// Tipo de um <see cref="Produtos.MovimentoStock"/>. A fonte de verdade
/// do estoque é sempre o histórico de movimentos, nunca um contador solto.
/// </summary>
public enum TipoMovimentoStock
{
    /// <summary>Aumenta o estoque (ex.: compra). Quantidade sempre positiva.</summary>
    Entrada = 1,

    /// <summary>Diminui o estoque (ex.: venda). Quantidade sempre negativa.</summary>
    Saida = 2,

    /// <summary>Correção manual (quebra, contagem física). Quantidade pode ser positiva ou negativa.</summary>
    Ajuste = 3
}
