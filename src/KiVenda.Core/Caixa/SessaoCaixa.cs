using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Caixa;

/// <summary>
/// Um turno de caixa: Abrir → vender/suprir/sangrar → Fechar. No fecho,
/// calcula-se a divergência entre o saldo esperado (calculado a partir
/// dos movimentos) e o saldo informado pelo operador.
/// </summary>
public sealed class SessaoCaixa : Entity
{
    private readonly List<MovimentoCaixa> _movimentos = new();

    public Guid UtilizadorAberturaId { get; private set; }

    public Guid? UtilizadorFechoId { get; private set; }

    public decimal SaldoInicial { get; private set; }

    public DateTime DataAbertura { get; private set; } = DateTime.UtcNow;

    public DateTime? DataFecho { get; private set; }

    public EstadoSessaoCaixa Estado { get; private set; } = EstadoSessaoCaixa.Aberta;

    /// <summary>Saldo final informado pelo operador ao fechar o caixa (contagem física).</summary>
    public decimal? SaldoFinalInformado { get; private set; }

    /// <summary>Diferença entre o saldo informado e o saldo calculado a partir dos movimentos (positivo = sobra, negativo = falta).</summary>
    public decimal? Divergencia { get; private set; }

    public IReadOnlyCollection<MovimentoCaixa> Movimentos => _movimentos.AsReadOnly();

    public decimal TotalEntradas => _movimentos.Where(m => m.EhEntrada).Sum(m => m.Valor);

    public decimal TotalSaidas => _movimentos.Where(m => !m.EhEntrada).Sum(m => m.Valor);

    /// <summary>Saldo esperado em caixa neste momento, calculado a partir do saldo inicial e dos movimentos.</summary>
    public decimal SaldoCalculado => SaldoInicial + TotalEntradas - TotalSaidas;

    private SessaoCaixa()
    {
    }

    public SessaoCaixa(Guid utilizadorAberturaId, decimal saldoInicial)
    {
        if (utilizadorAberturaId == Guid.Empty)
        {
            throw new DomainException("A sessão de caixa tem de estar associada a um utilizador.");
        }

        if (saldoInicial < 0)
        {
            throw new DomainException("O saldo inicial do caixa não pode ser negativo.");
        }

        UtilizadorAberturaId = utilizadorAberturaId;
        SaldoInicial = saldoInicial;
    }

    public MovimentoCaixa RegistarSuprimento(decimal valor, Guid utilizadorId, string? descricao = null)
    {
        return RegistarMovimento(TipoMovimentoCaixa.Suprimento, valor, utilizadorId, descricao, origemVendaId: null);
    }

    public MovimentoCaixa RegistarSangria(decimal valor, Guid utilizadorId, string? descricao = null)
    {
        GarantirAberta();

        if (valor > SaldoCalculado)
        {
            throw new DomainException($"Sangria de {valor} excede o saldo atual em caixa ({SaldoCalculado}).");
        }

        return RegistarMovimento(TipoMovimentoCaixa.Sangria, valor, utilizadorId, descricao, origemVendaId: null);
    }

    /// <summary>Regista a entrada automática correspondente a uma venda finalizada.</summary>
    public MovimentoCaixa RegistarEntradaDeVenda(decimal valor, Guid utilizadorId, Guid vendaId)
    {
        return RegistarMovimento(TipoMovimentoCaixa.Venda, valor, utilizadorId, descricao: null, origemVendaId: vendaId);
    }

    private MovimentoCaixa RegistarMovimento(TipoMovimentoCaixa tipo, decimal valor, Guid utilizadorId, string? descricao, Guid? origemVendaId)
    {
        GarantirAberta();

        var movimento = new MovimentoCaixa(Id, tipo, valor, utilizadorId, descricao, origemVendaId);
        _movimentos.Add(movimento);
        MarcarComoAtualizado();

        return movimento;
    }

    /// <summary>
    /// Fecha a sessão de caixa, registando o saldo informado pelo
    /// operador (contagem física) e calculando a divergência face ao
    /// saldo esperado a partir dos movimentos.
    /// </summary>
    public decimal Fechar(decimal saldoInformado, Guid utilizadorFechoId)
    {
        GarantirAberta();

        if (saldoInformado < 0)
        {
            throw new DomainException("O saldo informado no fecho não pode ser negativo.");
        }

        if (utilizadorFechoId == Guid.Empty)
        {
            throw new DomainException("O fecho de caixa tem de estar associado a um utilizador.");
        }

        SaldoFinalInformado = saldoInformado;
        Divergencia = saldoInformado - SaldoCalculado;
        UtilizadorFechoId = utilizadorFechoId;
        DataFecho = DateTime.UtcNow;
        Estado = EstadoSessaoCaixa.Fechada;
        MarcarComoAtualizado();

        return Divergencia.Value;
    }

    private void GarantirAberta()
    {
        if (Estado != EstadoSessaoCaixa.Aberta)
        {
            throw new DomainException("Esta operação só é permitida com a sessão de caixa aberta.");
        }
    }
}
