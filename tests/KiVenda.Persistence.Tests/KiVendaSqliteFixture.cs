using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace KiVenda.Persistence.Tests;

/// <summary>
/// Cria um <see cref="KiVendaDbContext"/> sobre SQLite em memória,
/// isolado por teste. Usa <c>EnsureCreated</c> (schema gerado
/// diretamente a partir do modelo), em vez de migrações reais — os
/// testes desta fase validam se o MAPEAMENTO das entidades está
/// correto, não o histórico de migrações (que exige a ferramenta
/// `dotnet ef`, indisponível neste ambiente de geração do scaffold; ver
/// README, secção "Pendente para validar").
///
/// SQLite em memória vive apenas enquanto a conexão estiver aberta, daí
/// a conexão ser mantida viva durante o tempo de vida do fixture.
/// </summary>
public sealed class KiVendaSqliteFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public KiVendaDbContext Context { get; }

    public KiVendaSqliteFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<KiVendaDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new KiVendaDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
