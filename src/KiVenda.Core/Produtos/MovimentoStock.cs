using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

/// <summary>
/// Fonte de verdade do estoque de um produto. O <see cref="Produto.EstoqueAtual"/>
/// é apenas um valor materializado (para leitura rápida); qualquer
/// divergência deve poder ser explicada recalculando a soma dos
/// movimentos de um produto.
///
/// A quantidade é sempre expressa na <see cref="UnidadeMedida"/> base do
/// produto (nunca na apresentação comercial) e é sinalizada:
///   Entrada → sempre positiva (aumenta o estoque)
///   Saida   → sempre negativa (diminui o estoque)
///   Ajuste  → positiva ou negativa (correção manual)
/// </summary>
public sealed class MovimentoStock : Entity
{
    public Guid ProdutoId { get; private set; }

    public TipoMovimentoStock Tipo { get; private set; }

    /// <summary>
    /// Quantidade sinalizada, em unidade base do produto.
    /// Positiva = aumenta o estoque; negativa = diminui.
    /// </summary>
    public decimal Quantidade { get; private set; }

    /// <summary>
    /// Custo por unidade base referente a esta entrada (ex.: 1,10 Kz/g).
    /// Só é definido em movimentos de <see cref="TipoMovimentoStock.Entrada"/>;
    /// é a partir deste valor que o custo médio ponderado do produto é
    /// recalculado.
    /// </summary>
    public decimal? CustoUnitarioUnidadeBase { get; private set; }

    public OrigemMovimentoStock Origem { get; private set; }

    /// <summary>Id da Compra, Venda ou nulo (ajuste manual), conforme <see cref="Origem"/>.</summary>
    public Guid? OrigemId { get; private set; }

    /// <summary>
    /// Referência opcional a um <see cref="Lote"/>. Preparado para uma
    /// futura política de custeio por lote/FIFO — não usado no MVP.
    /// </summary>
    public Guid? LoteId { get; private set; }

    public Guid UtilizadorId { get; private set; }

    /// <summary>Obrigatório para <see cref="TipoMovimentoStock.Ajuste"/> (ex.: "quebra", "contagem física").</summary>
    public string? Motivo { get; private set; }

    public DateTime Data { get; private set; } = DateTime.UtcNow;

    private MovimentoStock()
    {
    }

    private MovimentoStock(
        Guid produtoId,
        TipoMovimentoStock tipo,
        decimal quantidade,
        decimal? custoUnitarioUnidadeBase,
        OrigemMovimentoStock origem,
        Guid? origemId,
        Guid utilizadorId,
        string? motivo,
        Guid? loteId)
    {
        ProdutoId = produtoId;
        Tipo = tipo;
        Quantidade = quantidade;
        CustoUnitarioUnidadeBase = custoUnitarioUnidadeBase;
        Origem = origem;
        OrigemId = origemId;
        UtilizadorId = utilizadorId;
        Motivo = motivo;
        LoteId = loteId;
    }

    /// <summary>
    /// Cria um movimento de entrada (ex.: a partir de uma compra).
    /// A quantidade e o custo total devem já estar na unidade base do
    /// produto (a conversão a partir da apresentação comprada acontece
    /// antes, em <see cref="ApresentacaoProduto.ConverterParaUnidadeBase"/>).
    /// </summary>
    public static MovimentoStock CriarEntrada(
        Guid produtoId,
        decimal quantidadeUnidadeBase,
        decimal custoTotal,
        OrigemMovimentoStock origem,
        Guid? origemId,
        Guid utilizadorId,
        Guid? loteId = null)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um produto.");
        }

        if (quantidadeUnidadeBase <= 0)
        {
            throw new DomainException("A quantidade de uma entrada de stock tem de ser positiva.");
        }

        if (custoTotal < 0)
        {
            throw new DomainException("O custo total de uma entrada de stock não pode ser negativo.");
        }

        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um utilizador.");
        }

        var custoUnitario = custoTotal / quantidadeUnidadeBase;

        return new MovimentoStock(
            produtoId,
            TipoMovimentoStock.Entrada,
            quantidadeUnidadeBase,
            custoUnitario,
            origem,
            origemId,
            utilizadorId,
            motivo: null,
            loteId);
    }

    /// <summary>
    /// Cria um movimento de saída (ex.: a partir de uma venda). A
    /// quantidade deve já estar na unidade base do produto.
    /// </summary>
    public static MovimentoStock CriarSaida(
        Guid produtoId,
        decimal quantidadeUnidadeBase,
        OrigemMovimentoStock origem,
        Guid? origemId,
        Guid utilizadorId)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um produto.");
        }

        if (quantidadeUnidadeBase <= 0)
        {
            throw new DomainException("A quantidade de uma saída de stock tem de ser positiva (o sinal é aplicado internamente).");
        }

        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um utilizador.");
        }

        return new MovimentoStock(
            produtoId,
            TipoMovimentoStock.Saida,
            -quantidadeUnidadeBase,
            custoUnitarioUnidadeBase: null,
            origem,
            origemId,
            utilizadorId,
            motivo: null,
            loteId: null);
    }

    /// <summary>
    /// Cria um movimento de ajuste manual (correção de stock). O delta
    /// pode ser positivo (sobra encontrada) ou negativo (quebra), mas
    /// nunca zero, e exige sempre um motivo.
    /// </summary>
    public static MovimentoStock CriarAjuste(
        Guid produtoId,
        decimal deltaUnidadeBase,
        string motivo,
        Guid utilizadorId)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um produto.");
        }

        if (deltaUnidadeBase == 0)
        {
            throw new DomainException("Um ajuste de stock tem de ter uma quantidade diferente de zero.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new DomainException("Um ajuste de stock tem de ter um motivo.");
        }

        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("O movimento de stock tem de estar associado a um utilizador.");
        }

        return new MovimentoStock(
            produtoId,
            TipoMovimentoStock.Ajuste,
            deltaUnidadeBase,
            custoUnitarioUnidadeBase: null,
            OrigemMovimentoStock.AjusteManual,
            origemId: null,
            utilizadorId,
            motivo.Trim(),
            loteId: null);
    }
}
