using KiVenda.Core.Notificacoes;

namespace KiVenda.Application.Abstractions.Persistence;

public interface INotificacaoRepository
{
    Task<IReadOnlyList<Notificacao>> ListarPorUtilizadorAsync(Guid utilizadorId, int limite = 100, CancellationToken cancellationToken = default);
    Task<Notificacao?> ObterUltimaPorTipoAsync(Guid utilizadorId, string tipo, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, Guid utilizadorId, CancellationToken cancellationToken = default);
}