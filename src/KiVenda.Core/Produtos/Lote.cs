using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Produtos;

/// <summary>
/// Representa um lote de entrada de um produto (ex.: "Lote A, 25 kg de
/// açúcar, comprado a 25.000 Kz"). Existe no domínio desde a Fase 1 para
/// que <see cref="MovimentoStock"/> já possa referenciá-lo de forma
/// opcional, mas NÃO é usado operacionalmente no MVP: o V1 do KiVenda usa
/// custo médio ponderado (ver <see cref="Produto"/>), não custeio por
/// lote/FIFO. Fica preparado para uma fase futura sem exigir remodelação
/// do Core (ver "Fora de Escopo (V1.0)" no plano de implementação).
/// </summary>
public sealed class Lote : Entity
{
    public Guid ProdutoId { get; private set; }

    public string Codigo { get; private set; } = null!;

    public DateTime DataEntrada { get; private set; }

    public DateTime? DataValidade { get; private set; }

    private Lote()
    {
    }

    public Lote(Guid produtoId, string codigo, DateTime dataEntrada, DateTime? dataValidade = null)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("O lote tem de estar associado a um produto.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new DomainException("O código do lote é obrigatório.");
        }

        if (dataValidade.HasValue && dataValidade.Value < dataEntrada)
        {
            throw new DomainException("A data de validade não pode ser anterior à data de entrada.");
        }

        ProdutoId = produtoId;
        Codigo = codigo.Trim();
        DataEntrada = dataEntrada;
        DataValidade = dataValidade;
    }
}
