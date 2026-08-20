using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;

namespace KiVenda.Core.Compras;

/// <summary>
/// Registo formal de uma aquisição junto de um fornecedor. Cada item
/// desta compra é a origem de uma entrada de stock (ver
/// <see cref="Produto.RegistarEntradaStock"/>), mas a orquestração
/// entre "confirmar compra" e "dar entrada no stock de cada produto"
/// pertence à camada de Application (caso de uso RegistarCompra —
/// Fase 3), não a este agregado: cada aggregate root (Compra, Produto)
/// só se muta a si próprio.
/// </summary>
public sealed class Compra : Entity
{
    private readonly List<ItemCompra> _itens = new();

    public Guid FornecedorId { get; private set; }

    public Guid UtilizadorId { get; private set; }

    public DateTime Data { get; private set; } = DateTime.UtcNow;

    public IReadOnlyCollection<ItemCompra> Itens => _itens.AsReadOnly();

    public decimal CustoTotal => _itens.Sum(i => i.CustoTotalItem);

    private Compra()
    {
    }

    public Compra(Guid fornecedorId, Guid utilizadorId)
    {
        if (fornecedorId == Guid.Empty)
        {
            throw new DomainException("A compra tem de estar associada a um fornecedor.");
        }

        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("A compra tem de estar associada a um utilizador.");
        }

        FornecedorId = fornecedorId;
        UtilizadorId = utilizadorId;
    }

    /// <summary>
    /// Adiciona um item à compra, na apresentação efetivamente comprada
    /// (ex.: "saco de 25 kg"). A conversão para unidade base é feita
    /// aqui, para que o custo por unidade base fique disponível de
    /// imediato para validação do operador antes de confirmar.
    /// </summary>
    public ItemCompra AdicionarItem(Produto produto, Guid apresentacaoId, decimal quantidadeNaApresentacao, decimal custoTotalItem)
    {
        if (quantidadeNaApresentacao <= 0)
        {
            throw new DomainException("A quantidade comprada tem de ser positiva.");
        }

        var apresentacao = produto.ObterApresentacao(apresentacaoId);
        var item = new ItemCompra(produto, apresentacao, quantidadeNaApresentacao, custoTotalItem);

        _itens.Add(item);
        MarcarComoAtualizado();

        return item;
    }

    public void RemoverItem(Guid itemId)
    {
        var item = _itens.FirstOrDefault(i => i.Id == itemId);

        if (item is null)
        {
            throw new DomainException("Item de compra não encontrado.");
        }

        _itens.Remove(item);
        MarcarComoAtualizado();
    }
}
