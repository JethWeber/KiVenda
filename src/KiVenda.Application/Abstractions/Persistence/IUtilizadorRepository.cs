using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Abstractions.Persistence;

public interface IUtilizadorRepository
{
    Task<Utilizador?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Utilizador?> ObterPorNomeUtilizadorAsync(string nomeUtilizador, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Utilizador>> ListarAsync(bool apenasAtivos = true, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Utilizador utilizador, CancellationToken cancellationToken = default);
}
