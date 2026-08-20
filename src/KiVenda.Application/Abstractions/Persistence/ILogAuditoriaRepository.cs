using KiVenda.Core.Auditoria;

namespace KiVenda.Application.Abstractions.Persistence;

public interface ILogAuditoriaRepository
{
    Task AdicionarAsync(LogAuditoria log, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LogAuditoria>> ListarAsync(
        Guid? utilizadorId = null,
        string? entidadeAfetada = null,
        DateTime? de = null,
        DateTime? ate = null,
        int pagina = 1,
        int tamanhoPagina = 50,
        CancellationToken cancellationToken = default);
}
