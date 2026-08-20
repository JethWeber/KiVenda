using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

/// <summary>
/// Agregado raiz do estoque. Guarda e movimenta sempre na sua
/// <see cref="UnidadeMedida"/> base — nunca na apresentação comercial.
/// <see cref="EstoqueAtual"/> e <see cref="CustoMedioPonderado"/> são
/// valores materializados, recalculados a cada <see cref="MovimentoStock"/>;
/// a fonte de verdade é sempre o histórico de movimentos (ver
/// <see cref="MovimentoStock"/> e a Nota de Revisão de Domínio — Estoque
/// no plano de implementação).
/// </summary>
public sealed class Produto : Entity
{
    private readonly List<ApresentacaoProduto> _apresentacoes = new();

    public string Nome { get; private set; } = null!;

    public string CodigoInterno { get; private set; } = null!;

    public string? CodigoBarras { get; private set; }

    public Guid CategoriaId { get; private set; }

    public Guid UnidadeBaseId { get; private set; }

    /// <summary>Preço de venda por unidade base (ex.: Kz por grama).</summary>
    public decimal PrecoVendaPorUnidadeBase { get; private set; }

    /// <summary>Stock mínimo, em unidade base, a partir do qual o produto passa a "Stock Baixo".</summary>
    public decimal StockMinimo { get; private set; }

    public string? FotoUrl { get; private set; }

    public bool Ativo { get; private set; } = true;

    /// <summary>
    /// Valor materializado do estoque atual, em unidade base. Atualizado
    /// exclusivamente através de <see cref="RegistarEntradaStock"/>,
    /// <see cref="RegistarSaidaStock"/> e <see cref="RegistarAjusteStock"/>
    /// — nunca escrito diretamente fora deste mecanismo.
    /// </summary>
    public decimal EstoqueAtual { get; private set; }

    /// <summary>
    /// Custo médio ponderado por unidade base, recalculado a cada nova
    /// entrada de stock. É esta a base do cálculo de lucro — nunca um
    /// "preço de compra" fixo.
    /// </summary>
    public decimal CustoMedioPonderado { get; private set; }

    public IReadOnlyCollection<ApresentacaoProduto> Apresentacoes => _apresentacoes.AsReadOnly();

    private Produto()
    {
    }

    public Produto(
        string nome,
        string codigoInterno,
        Guid categoriaId,
        Guid unidadeBaseId,
        decimal precoVendaPorUnidadeBase,
        decimal stockMinimo,
        string? codigoBarras = null,
        string? fotoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(codigoInterno))
        {
            throw new DomainException("O código interno do produto é obrigatório.");
        }

        if (categoriaId == Guid.Empty)
        {
            throw new DomainException("O produto tem de ter uma categoria.");
        }

        if (unidadeBaseId == Guid.Empty)
        {
            throw new DomainException("O produto tem de ter uma unidade de medida base.");
        }

        if (precoVendaPorUnidadeBase < 0)
        {
            throw new DomainException("O preço de venda não pode ser negativo.");
        }

        if (stockMinimo < 0)
        {
            throw new DomainException("O stock mínimo não pode ser negativo.");
        }

        Nome = nome.Trim();
        CodigoInterno = codigoInterno.Trim();
        CategoriaId = categoriaId;
        UnidadeBaseId = unidadeBaseId;
        PrecoVendaPorUnidadeBase = precoVendaPorUnidadeBase;
        StockMinimo = stockMinimo;
        CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();
        FotoUrl = fotoUrl;

        // Todo produto nasce com uma apresentação "padrão" equivalente à
        // própria unidade base (fator 1), para que possa ser sempre
        // comprado/vendido mesmo sem apresentações adicionais.
        _apresentacoes.Add(new ApresentacaoProduto(Id, "Unidade base", fatorConversaoParaUnidadeBase: 1m, codigoBarras));
    }

    // ---------------------------------------------------------------
    // Cadastro
    // ---------------------------------------------------------------

    public void EditarDadosBasicos(string nome, decimal precoVendaPorUnidadeBase, decimal stockMinimo, string? codigoBarras, string? fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("O nome do produto é obrigatório.");
        }

        if (precoVendaPorUnidadeBase < 0)
        {
            throw new DomainException("O preço de venda não pode ser negativo.");
        }

        if (stockMinimo < 0)
        {
            throw new DomainException("O stock mínimo não pode ser negativo.");
        }

        Nome = nome.Trim();
        PrecoVendaPorUnidadeBase = precoVendaPorUnidadeBase;
        StockMinimo = stockMinimo;
        CodigoBarras = string.IsNullOrWhiteSpace(codigoBarras) ? null : codigoBarras.Trim();
        FotoUrl = fotoUrl;
        MarcarComoAtualizado();
    }

    public void AlterarCategoria(Guid categoriaId)
    {
        if (categoriaId == Guid.Empty)
        {
            throw new DomainException("O produto tem de ter uma categoria.");
        }

        CategoriaId = categoriaId;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Inativa o produto em vez de o eliminar. Um produto com movimentos
    /// de stock associados não pode ser apagado (ver Fase 1 do plano).
    /// </summary>
    public void Inativar()
    {
        Ativo = false;
        MarcarComoAtualizado();
    }

    public void Reativar()
    {
        Ativo = true;
        MarcarComoAtualizado();
    }

    // ---------------------------------------------------------------
    // Apresentações comerciais
    // ---------------------------------------------------------------

    public ApresentacaoProduto AdicionarApresentacao(string nome, decimal fatorConversaoParaUnidadeBase, string? codigoBarras = null)
    {
        if (_apresentacoes.Any(a => a.Ativa && string.Equals(a.Nome, nome, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException($"O produto já tem uma apresentação ativa chamada \"{nome}\".");
        }

        var apresentacao = new ApresentacaoProduto(Id, nome, fatorConversaoParaUnidadeBase, codigoBarras);
        _apresentacoes.Add(apresentacao);
        MarcarComoAtualizado();

        return apresentacao;
    }

    public ApresentacaoProduto ObterApresentacao(Guid apresentacaoId)
    {
        var apresentacao = _apresentacoes.FirstOrDefault(a => a.Id == apresentacaoId);

        if (apresentacao is null)
        {
            throw new DomainException("Apresentação não encontrada para este produto.");
        }

        return apresentacao;
    }

    // ---------------------------------------------------------------
    // Movimentação de stock (fonte de verdade: MovimentoStock)
    // ---------------------------------------------------------------

    /// <summary>
    /// Regista uma entrada de stock (tipicamente a partir de uma compra),
    /// atualizando o estoque materializado e recalculando o custo médio
    /// ponderado do produto.
    /// </summary>
    public MovimentoStock RegistarEntradaStock(
        decimal quantidadeUnidadeBase,
        decimal custoTotal,
        OrigemMovimentoStock origem,
        Guid? origemId,
        Guid utilizadorId,
        Guid? loteId = null)
    {
        var movimento = MovimentoStock.CriarEntrada(Id, quantidadeUnidadeBase, custoTotal, origem, origemId, utilizadorId, loteId);

        AtualizarCustoMedioPonderado(quantidadeUnidadeBase, custoTotal);
        EstoqueAtual += quantidadeUnidadeBase;
        MarcarComoAtualizado();

        return movimento;
    }

    /// <summary>
    /// Regista uma saída de stock (tipicamente a partir de uma venda).
    /// Não permite que o estoque materializado fique negativo.
    /// </summary>
    public MovimentoStock RegistarSaidaStock(
        decimal quantidadeUnidadeBase,
        OrigemMovimentoStock origem,
        Guid? origemId,
        Guid utilizadorId)
    {
        if (quantidadeUnidadeBase > EstoqueAtual)
        {
            throw new DomainException(
                $"Stock insuficiente para \"{Nome}\": disponível {EstoqueAtual}, solicitado {quantidadeUnidadeBase}.");
        }

        var movimento = MovimentoStock.CriarSaida(Id, quantidadeUnidadeBase, origem, origemId, utilizadorId);

        EstoqueAtual -= quantidadeUnidadeBase;
        MarcarComoAtualizado();

        return movimento;
    }

    /// <summary>
    /// Regista um ajuste manual de stock (quebra ou sobra), sempre com
    /// motivo. Não permite que o estoque materializado fique negativo.
    /// </summary>
    public MovimentoStock RegistarAjusteStock(decimal deltaUnidadeBase, string motivo, Guid utilizadorId)
    {
        var resultante = EstoqueAtual + deltaUnidadeBase;

        if (resultante < 0)
        {
            throw new DomainException(
                $"O ajuste deixaria o stock de \"{Nome}\" negativo (atual {EstoqueAtual}, delta {deltaUnidadeBase}).");
        }

        var movimento = MovimentoStock.CriarAjuste(Id, deltaUnidadeBase, motivo, utilizadorId);

        EstoqueAtual = resultante;
        MarcarComoAtualizado();

        return movimento;
    }

    /// <summary>
    /// Recalcula o estoque materializado a partir do zero, somando o
    /// histórico completo de <see cref="MovimentoStock"/> deste produto.
    /// Usado para diagnóstico/correção de divergências (Fase 3:
    /// RecalcularEstoqueMaterializado). Não recalcula o custo médio
    /// ponderado, que depende apenas das entradas ainda "em stock" — se
    /// necessário, deve ser resolvido separadamente por quem chama.
    /// </summary>
    public void RecalcularEstoqueMaterializado(IEnumerable<MovimentoStock> movimentos)
    {
        var estoqueRecalculado = movimentos
            .Where(m => m.ProdutoId == Id)
            .Sum(m => m.Quantidade);

        EstoqueAtual = estoqueRecalculado;
        MarcarComoAtualizado();
    }

    private void AtualizarCustoMedioPonderado(decimal quantidadeEntrada, decimal custoTotalEntrada)
    {
        var custoAtualTotal = EstoqueAtual * CustoMedioPonderado;
        var novoEstoqueTotal = EstoqueAtual + quantidadeEntrada;

        CustoMedioPonderado = novoEstoqueTotal == 0
            ? 0
            : (custoAtualTotal + custoTotalEntrada) / novoEstoqueTotal;
    }

    // ---------------------------------------------------------------
    // Consultas / regras derivadas
    // ---------------------------------------------------------------

    public EstadoStock ObterEstadoStock()
    {
        if (EstoqueAtual <= 0)
        {
            return EstadoStock.SemStock;
        }

        return EstoqueAtual <= StockMinimo ? EstadoStock.StockBaixo : EstadoStock.EmStock;
    }

    /// <summary>
    /// Lucro estimado para uma quantidade (em unidade base) vendida ao
    /// preço de venda atual, usando o custo médio ponderado corrente —
    /// nunca um "preço de compra" fixo.
    /// </summary>
    public decimal CalcularLucroEstimado(decimal quantidadeUnidadeBase)
    {
        return (PrecoVendaPorUnidadeBase - CustoMedioPonderado) * quantidadeUnidadeBase;
    }

    /// <summary>Valor do estoque atual, a custo médio ponderado (usado no relatório de inventário).</summary>
    public decimal CalcularValorEstoque() => EstoqueAtual * CustoMedioPonderado;
}
