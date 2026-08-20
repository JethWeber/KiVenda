using KiVenda.Core.Vendas;

namespace KiVenda.Application.Abstractions.Persistence;

public interface IVendaRepository
{
    Task<Venda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Venda>> ListarAsync(
        Guid? utilizadorId = null,
        Guid? clienteId = null,
        DateTime? de = null,
        DateTime? ate = null,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Venda venda, CancellationToken cancellationToken = default);
}
