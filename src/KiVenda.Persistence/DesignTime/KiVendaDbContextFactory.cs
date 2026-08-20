using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KiVenda.Persistence.DesignTime;

/// <summary>
/// Usada exclusivamente pelas ferramentas do EF Core em tempo de design
/// (ex.: <c>dotnet ef migrations add</c>), que precisam de construir um
/// <see cref="KiVendaDbContext"/> sem depender do composition root da
/// aplicação Desktop (Fase 0/6). Nunca é usada em runtime — a app real
/// resolve a connection string através de
/// <c>KiVenda.Persistence.DependencyInjection.ServiceCollectionExtensions.AddPersistence</c>.
///
/// Para gerar a migração inicial da Fase 2, a partir da raiz do
/// repositório:
///   dotnet ef migrations add InicialCreate --project src/KiVenda.Persistence --startup-project src/KiVenda.Persistence
/// </summary>
public sealed class KiVendaDbContextFactory : IDesignTimeDbContextFactory<KiVendaDbContext>
{
    public KiVendaDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KiVendaDbContext>();
        optionsBuilder.UseSqlite("Data Source=kivenda.designtime.db");

        return new KiVendaDbContext(optionsBuilder.Options);
    }
}
