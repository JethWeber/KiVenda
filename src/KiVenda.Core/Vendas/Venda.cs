using KiVenda.Core.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;

namespace KiVenda.Core.Vendas;

/// <summary>
/// Módulo central do sistema. A saída de stock correspondente a cada
/// item (via <see cref="Produto.RegistarSaidaStock"/>) é orquestrada
/// pela camada de Application ao finalizar a venda (caso de uso
/// FinalizarVenda — Fase 3), não por este agregado.
/// </summary>
public sealed class Venda : Entity
{
    private readonly List<ItemVenda> _itens = new();
    private readonly List<Pagamento> _pagamentos = new();

    public Guid? ClienteId { get; private set; }

    public Guid UtilizadorId { get; private set; }

    public Guid SessaoCaixaId { get; private set; }

    public DateTime Data { get; private set; } = DateTime.UtcNow;

    public decimal Desconto { get; private set; }

    public EstadoVenda Estado { get; private set; } = EstadoVenda.EmAndamento;

    public IReadOnlyCollection<ItemVenda> Itens => _itens.AsReadOnly();

    public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();

    public decimal Subtotal => _itens.Sum(i => i.ValorTotal);

    public decimal Total => Math.Max(0, Subtotal - Desconto);

    public decimal TotalPago => _pagamentos.Sum(p => p.Valor);

    /// <summary>
    /// Lucro estimado da venda: soma do lucro de cada item, sem ajuste
    /// pelo desconto aplicado (o desconto é tratado como redução de
    /// margem comercial, reportado separadamente nos relatórios — Fase 9).
    /// </summary>
    public decimal LucroEstimado => _itens.Sum(i => i.LucroEstimado);

    private Venda()
    {
    }

    public Venda(Guid utilizadorId, Guid sessaoCaixaId, Guid? clienteId = null)
    {
        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("A venda tem de estar associada a um utilizador.");
        }

        if (sessaoCaixaId == Guid.Empty)
        {
            throw new DomainException("A venda tem de estar associada a uma sessão de caixa aberta.");
        }

        UtilizadorId = utilizadorId;
        SessaoCaixaId = sessaoCaixaId;
        ClienteId = clienteId;
    }

    public ItemVenda AdicionarItem(Produto produto, Guid apresentacaoId, decimal quantidadeNaApresentacao)
    {
        GarantirEmAndamento();

        var apresentacao = produto.ObterApresentacao(apresentacaoId);
        var item = new ItemVenda(produto, apresentacao, quantidadeNaApresentacao);

        _itens.Add(item);
        MarcarComoAtualizado();

        return item;
    }

    public void RemoverItem(Guid itemId)
    {
        GarantirEmAndamento();

        var item = _itens.FirstOrDefault(i => i.Id == itemId);

        if (item is null)
        {
            throw new DomainException("Item de venda não encontrado.");
        }

        _itens.Remove(item);
        MarcarComoAtualizado();
    }

    public void AplicarDesconto(decimal valor)
    {
        GarantirEmAndamento();

        if (valor < 0)
        {
            throw new DomainException("O desconto não pode ser negativo.");
        }

        if (valor > Subtotal)
        {
            throw new DomainException("O desconto não pode ser maior do que o subtotal da venda.");
        }

        Desconto = valor;
        MarcarComoAtualizado();
    }

    public Pagamento AdicionarPagamento(MetodoPagamento metodo, decimal valor)
    {
        GarantirEmAndamento();

        var pagamento = new Pagamento(Id, metodo, valor);
        _pagamentos.Add(pagamento);
        MarcarComoAtualizado();

        return pagamento;
    }

    /// <summary>
    /// Finaliza a venda. Exige pelo menos um item e que o total pago
    /// cubra o total da venda. A baixa de stock por item é feita pela
    /// Application a partir daqui (ver <see cref="Itens"/>), não por
    /// este método.
    /// </summary>
    public void Finalizar()
    {
        GarantirEmAndamento();

        if (_itens.Count == 0)
        {
            throw new DomainException("A venda tem de ter pelo menos um item para ser finalizada.");
        }

        if (TotalPago < Total)
        {
            throw new DomainException($"Pagamento insuficiente: total {Total}, pago {TotalPago}.");
        }

        Estado = EstadoVenda.Finalizada;
        MarcarComoAtualizado();
    }

    public void Cancelar()
    {
        if (Estado == EstadoVenda.Finalizada)
        {
            throw new DomainException("Uma venda já finalizada não pode ser cancelada por aqui — ver processo de estorno.");
        }

        Estado = EstadoVenda.Cancelada;
        MarcarComoAtualizado();
    }

    private void GarantirEmAndamento()
    {
        if (Estado != EstadoVenda.EmAndamento)
        {
            throw new DomainException("Esta operação só é permitida enquanto a venda está em andamento.");
        }
    }
}
