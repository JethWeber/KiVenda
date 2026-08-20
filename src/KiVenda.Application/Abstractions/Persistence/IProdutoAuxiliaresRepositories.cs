using KiVenda.Core.Produtos;

namespace KiVenda.Application.Abstractions.Persistence;

public interface ICategoriaRepository
{
    Task<Categoria?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default);

    Task AdicionarAsync(Categoria categoria, CancellationToken cancellationToken = default);
}

public interface IUnidadeMedidaRepository
{
    Task<UnidadeMedida?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UnidadeMedida?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UnidadeMedida>> ListarAsync(CancellationToken cancellationToken = default);

    Task AdicionarAsync(UnidadeMedida unidadeMedida, CancellationToken cancellationToken = default);
}
