using KiVenda.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Persistence.DependencyInjection;

/// <summary>
/// Ponto único de registo desta camada no composition root da aplicação
/// (KiVenda.Desktop, a partir da Fase 6). Mantém o <c>App.axaml.cs</c>
/// limpo: basta chamar <c>services.AddPersistence(caminhoBaseDeDados)</c>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Regista o <see cref="KiVendaDbContext"/> sobre SQLite e o
    /// <see cref="IUnitOfWork"/>.
    /// </summary>
    /// <param name="caminhoBaseDeDados">
    /// Caminho absoluto do ficheiro .db local (ex.: pasta de dados da
    /// aplicação no perfil do utilizador). A geração automática deste
    /// caminho e a aplicação das migrações no arranque ("auto-provisioning",
    /// Secção 3 da documentação funcional) são responsabilidade do
    /// composition root do Desktop, não desta extensão.
    /// </param>
    public static IServiceCollection AddPersistence(this IServiceCollection services, string caminhoBaseDeDados)
    {
        services.AddDbContext<KiVendaDbContext>(options =>
            options.UseSqlite($"Data Source={caminhoBaseDeDados}"));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
