using KiVenda.Core.Notificacoes;

namespace KiVenda.Application.Abstractions.Persistence;

public interface INotificacaoRepository
{
    Task<IReadOnlyList<Notificacao>> ListarPorUtilizadorAsync(Guid utilizadorId, int limite = 100, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default);
}
