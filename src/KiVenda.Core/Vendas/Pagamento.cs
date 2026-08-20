using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Vendas;

/// <summary>
/// Um pagamento associado a uma <see cref="Venda"/>. Uma venda pode ter
/// mais do que um pagamento (pagamento misto — ex.: parte em Dinheiro,
/// parte em TPA).
/// </summary>
public sealed class Pagamento : Entity
{
    public Guid VendaId { get; private set; }

    public MetodoPagamento Metodo { get; private set; }

    public decimal Valor { get; private set; }

    private Pagamento()
    {
    }

    internal Pagamento(Guid vendaId, MetodoPagamento metodo, decimal valor)
    {
        if (vendaId == Guid.Empty)
        {
            throw new DomainException("O pagamento tem de estar associado a uma venda.");
        }

        if (valor <= 0)
        {
            throw new DomainException("O valor do pagamento tem de ser positivo.");
        }

        VendaId = vendaId;
        Metodo = metodo;
        Valor = valor;
    }
}
