using KiVenda.Core.Enums;

namespace KiVenda.Application.Produtos;

public sealed record ApresentacaoProdutoDto(
    Guid Id,
    string Nome,
    decimal FatorConversaoParaUnidadeBase,
    string? CodigoBarras,
    bool Ativa);

/// <summary>
/// Representação de um produto para a UI. Os valores de estoque e custo
/// vêm sempre em unidade base — a conversão para uma apresentação
/// legível (ex.: "23,3 kg" em vez de "23300 g") é responsabilidade da
/// UI (Fase 6), usando <see cref="ApresentacaoProdutoDto.FatorConversaoParaUnidadeBase"/>.
/// </summary>
public sealed record ProdutoDto(
    Guid Id,
    string Nome,
    string CodigoInterno,
    string? CodigoBarras,
    Guid CategoriaId,
    Guid UnidadeBaseId,
    decimal PrecoVendaPorUnidadeBase,
    decimal StockMinimo,
    decimal EstoqueAtual,
    decimal CustoMedioPonderado,
    decimal ValorEstoque,
    EstadoStock EstadoStock,
    bool Ativo,
    IReadOnlyList<ApresentacaoProdutoDto> Apresentacoes);
