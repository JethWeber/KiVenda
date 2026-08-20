using KiVenda.Core.Clientes;

namespace KiVenda.Application.Abstractions.Persistence;

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cliente>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
