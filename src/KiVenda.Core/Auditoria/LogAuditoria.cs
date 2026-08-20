using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Auditoria;

/// <summary>
/// Registo de uma operação sensível realizada no sistema (Secção 7 da
/// documentação funcional). Protege o gerente ao permitir identificar
/// quem fez o quê e quando — especialmente útil em caso de divergência
/// de caixa ou alteração indevida de dados.
/// </summary>
public sealed class LogAuditoria : Entity
{
    public Guid UtilizadorId { get; private set; }

    /// <summary>Descrição curta da ação (ex.: "Venda realizada", "Alterou preço", "Eliminou Produto").</summary>
    public string Acao { get; private set; } = null!;

    public string EntidadeAfetada { get; private set; } = null!;

    public Guid? EntidadeId { get; private set; }

    /// <summary>Estado relevante antes da operação, quando aplicável (ex.: preço anterior).</summary>
    public string? DadosAntes { get; private set; }

    /// <summary>Estado relevante depois da operação, quando aplicável (ex.: novo preço).</summary>
    public string? DadosDepois { get; private set; }

    public DateTime DataHora { get; private set; } = DateTime.UtcNow;

    private LogAuditoria()
    {
    }

    public LogAuditoria(Guid utilizadorId, string acao, string entidadeAfetada, Guid? entidadeId = null, string? dadosAntes = null, string? dadosDepois = null)
    {
        if (utilizadorId == Guid.Empty)
        {
            throw new DomainException("O registo de auditoria tem de estar associado a um utilizador.");
        }

        if (string.IsNullOrWhiteSpace(acao))
        {
            throw new DomainException("O registo de auditoria tem de descrever a ação realizada.");
        }

        if (string.IsNullOrWhiteSpace(entidadeAfetada))
        {
            throw new DomainException("O registo de auditoria tem de indicar a entidade afetada.");
        }

        UtilizadorId = utilizadorId;
        Acao = acao.Trim();
        EntidadeAfetada = entidadeAfetada.Trim();
        EntidadeId = entidadeId;
        DadosAntes = dadosAntes;
        DadosDepois = dadosDepois;
    }
}
