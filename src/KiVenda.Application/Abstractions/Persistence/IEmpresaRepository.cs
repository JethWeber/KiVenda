using KiVenda.Core.Empresas;

namespace KiVenda.Application.Abstractions.Persistence;

public interface IEmpresaRepository
{
    Task<Empresa?> ObterAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Empresa empresa, CancellationToken cancellationToken = default);
    void Remover(Empresa empresa);
}
