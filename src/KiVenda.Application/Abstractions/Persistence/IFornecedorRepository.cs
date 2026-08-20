using KiVenda.Core.Fornecedores;

namespace KiVenda.Application.Abstractions.Persistence;

public interface IFornecedorRepository
{
    Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Fornecedor>> ListarAsync(string? termoPesquisa = null, CancellationToken cancellationToken = default);

    Task AdicionarAsync(Fornecedor fornecedor, CancellationToken cancellationToken = default);
}
