using KiVenda.Core.Caixa;

namespace KiVenda.Application.Abstractions.Persistence;

public interface ISessaoCaixaRepository
{
    Task<SessaoCaixa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Sessão de caixa atualmente aberta (o MVP assume um único caixa aberto de cada vez — ver Fase 5/7).</summary>
    Task<SessaoCaixa?> ObterAbertaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessaoCaixa>> ListarAsync(DateTime? de = null, DateTime? ate = null, CancellationToken cancellationToken = default);

    Task AdicionarAsync(SessaoCaixa sessaoCaixa, CancellationToken cancellationToken = default);
}
