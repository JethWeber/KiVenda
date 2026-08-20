using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

public class KiVendaDbSeederTests
{
    [Fact]
    public async Task SeedAsync_Deve_Criar_Unidades_Categoria_Geral_E_Gerente_Padrao()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        await KiVendaDbSeeder.SeedAsync(context, senhaGerentePadraoHash: "hash-qualquer");

        var codigosUnidades = await context.UnidadesMedida.Select(u => u.Codigo).ToListAsync();
        codigosUnidades.Should().Contain(new[] { "un", "g", "ml" });

        (await context.Categorias.AnyAsync(c => c.Nome == "Geral")).Should().BeTrue();

        var gerente = await context.Utilizadores.FirstOrDefaultAsync(u => u.Perfil == PerfilUtilizador.Gerente);
        gerente.Should().NotBeNull();
        gerente!.NomeUtilizador.Should().Be("gerente");
    }

    [Fact]
    public async Task SeedAsync_Deve_Ser_Idempotente_Ao_Ser_Chamado_Duas_Vezes()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        await KiVendaDbSeeder.SeedAsync(context, "hash-qualquer");
        await KiVendaDbSeeder.SeedAsync(context, "hash-qualquer");

        (await context.UnidadesMedida.CountAsync()).Should().Be(3);
        (await context.Categorias.CountAsync(c => c.Nome == "Geral")).Should().Be(1);
        (await context.Utilizadores.CountAsync(u => u.Perfil == PerfilUtilizador.Gerente)).Should().Be(1);
    }
}
