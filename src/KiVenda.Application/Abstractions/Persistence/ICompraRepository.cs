using KiVenda.Core.Compras;

namespace KiVenda.Application.Abstractions.Persistence;

public interface ICompraRepository
{
    Task<Compra?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Compra>> ListarAsync(
        Guid? fornecedorId = null,
        DateTime? de = null,
        DateTime? ate = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Compra compra, CancellationToken cancellationToken = default);
}
