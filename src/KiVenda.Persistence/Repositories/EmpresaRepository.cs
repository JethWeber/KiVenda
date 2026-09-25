using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Empresas;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Repositories;

internal sealed class EmpresaRepository(KiVendaDbContext context) : IEmpresaRepository
{
    public Task<Empresa?> ObterAsync(CancellationToken cancellationToken = default) =>
        context.Empresas.FirstOrDefaultAsync(cancellationToken);

    public async Task AdicionarAsync(Empresa empresa, CancellationToken cancellationToken = default) =>
        await context.Empresas.AddAsync(empresa, cancellationToken);

    public void Remover(Empresa empresa) => context.Empresas.Remove(empresa);
}
