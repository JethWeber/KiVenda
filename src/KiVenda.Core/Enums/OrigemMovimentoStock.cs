namespace KiVenda.Core.Enums;

/// <summary>
/// De onde partiu um <see cref="Produtos.MovimentoStock"/>, usado para
/// rastreabilidade (ex.: em caso de divergência, saber se a saída veio
/// de uma venda específica ou de um ajuste manual).
/// </summary>
public enum OrigemMovimentoStock
{
    Compra = 1,
    Venda = 2,
    AjusteManual = 3
}
