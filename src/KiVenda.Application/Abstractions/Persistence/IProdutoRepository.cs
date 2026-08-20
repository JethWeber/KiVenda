using KiVenda.Core.Produtos;

namespace KiVenda.Application.Abstractions.Persistence;

/// <summary>
/// Contrato de acesso a dados para <see cref="Produto"/>, implementado
/// por <c>KiVenda.Persistence</c> (Fase 2). Definido aqui, em vez de em
/// Persistence, para respeitar a inversão de dependência: quem consome
/// (Application, a partir da Fase 3) é dono do contrato.
/// </summary>
public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Procura um produto pelo código de barras — tanto o código de
    /// barras principal do produto como o de qualquer uma das suas
    /// apresentações comerciais (ex.: o EAN do saco fechado de 1 kg).
    /// Usado pelo fluxo de scanner (Fase 8).
    /// </summary>
    Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras, CancellationToken cancellationToken = default);

    Task<Produto?> ObterPorCodigoInternoAsync(string codigoInterno, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Produto>> ListarAsync(
        string? termoPesquisa = null,
        Guid? categoriaId = null,
        bool apenasAtivos = true,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default);
}
