using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

public class UtilizadorPersistenceTests
{
    [Fact]
    public async Task NomeUtilizador_Deve_Ser_Unico()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        await context.Utilizadores.AddAsync(new Utilizador("Maria João", "maria", "hash-1", PerfilUtilizador.Gerente));
        await context.SaveChangesAsync();

        await context.Utilizadores.AddAsync(new Utilizador("Maria Silva", "maria", "hash-2", PerfilUtilizador.Atendente));

        var acao = async () => await context.SaveChangesAsync();

        await acao.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Utilizador_Deve_Ser_Recarregado_Com_O_Perfil_Correto()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var utilizador = new Utilizador("João", "joao", "hash-qualquer", PerfilUtilizador.Atendente);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregado = await context.Utilizadores.AsNoTracking().FirstAsync(u => u.Id == utilizador.Id);

        recarregado.Perfil.Should().Be(PerfilUtilizador.Atendente);
        recarregado.PodeExecutar(Acao.FazerVenda).Should().BeTrue();
        recarregado.PodeExecutar(Acao.CriarUtilizadores).Should().BeFalse();
    }
}
