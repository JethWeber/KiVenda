using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Caixa;

/// <summary>
/// Um movimento dentro de uma <see cref="SessaoCaixa"/>: suprimento
/// (entrada manual), sangria (saída manual) ou o valor recebido numa
/// venda finalizada (entrada automática).
/// </summary>
public sealed class MovimentoCaixa : Entity
{
    public Guid SessaoCaixaId { get; private set; }

    public TipoMovimentoCaixa Tipo { get; private set; }

    /// <summary>Valor sempre positivo; o sinal (entrada/saída) é dado por <see cref="Tipo"/>.</summary>
    public decimal Valor { get; private set; }

    public Guid UtilizadorId { get; private set; }

    public string? Descricao { get; private set; }

    /// <summary>Preenchido apenas quando <see cref="Tipo"/> é <see cref="TipoMovimentoCaixa.Venda"/>.</summary>
    public Guid? OrigemVendaId { get; private set; }

    public DateTime Data { get; private set; } = DateTime.UtcNow;

    private MovimentoCaixa()
    {
    }

    internal MovimentoCaixa(Guid sessaoCaixaId, TipoMovimentoCaixa tipo, decimal valor, Guid utilizadorId, string? descricao, Guid? origemVendaId)
    {
        if (sessaoCaixaId == Guid.Empty)
        {
            throw new DomainException("O movimento de caixa tem de estar associado a uma sessão de caixa.");
        }

        if (valor <= 0)
        {
            throw new DomainException("O valor de um movimento de caixa tem de ser positivo.");
        }

        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("O movimento de caixa tem de estar associado a um utilizador.");
        }

        if (tipo == TipoMovimentoCaixa.Venda && origemVendaId is null)
        {
            throw new DomainException("Um movimento de caixa de tipo Venda tem de referenciar a venda de origem.");
        }

        SessaoCaixaId = sessaoCaixaId;
        Tipo = tipo;
        Valor = valor;
        UtilizadorId = utilizadorId;
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        OrigemVendaId = origemVendaId;
    }

    public bool EhEntrada => Tipo is TipoMovimentoCaixa.Suprimento or TipoMovimentoCaixa.Venda;
}
