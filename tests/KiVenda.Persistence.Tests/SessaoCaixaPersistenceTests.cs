using FluentAssertions;
using KiVenda.Core.Caixa;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

public class SessaoCaixaPersistenceTests
{
    [Fact]
    public async Task SessaoCaixa_Com_Movimentos_Deve_Ser_Persistida_E_Recarregada_Com_Saldo_Correto()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var utilizador = new Utilizador("Maria", "maria", "hash-qualquer", PerfilUtilizador.Gerente);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();

        var sessao = new SessaoCaixa(utilizador.Id, saldoInicial: 20000m);
        sessao.RegistarSuprimento(50000m, utilizador.Id, "Reforço de troco");
        sessao.RegistarSangria(35000m, utilizador.Id, "Recolha para depósito");

        await context.SessoesCaixa.AddAsync(sessao);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregada = await context.SessoesCaixa
            .Include(s => s.Movimentos)
            .AsNoTracking()
            .FirstAsync(s => s.Id == sessao.Id);

        recarregada.Movimentos.Should().HaveCount(2);
        recarregada.SaldoCalculado.Should().Be(35000m); // 20000 + 50000 - 35000
    }

    [Fact]
    public async Task Fecho_De_Caixa_Deve_Persistir_Divergencia_Calculada()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var utilizador = new Utilizador("Maria", "maria", "hash-qualquer", PerfilUtilizador.Gerente);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();

        var sessao = new SessaoCaixa(utilizador.Id, saldoInicial: 20000m);
        sessao.RegistarSuprimento(50000m, utilizador.Id);
        sessao.Fechar(saldoInformado: 69500m, utilizador.Id); // esperado 70000, informado 69500 -> divergência -500

        await context.SessoesCaixa.AddAsync(sessao);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregada = await context.SessoesCaixa.AsNoTracking().FirstAsync(s => s.Id == sessao.Id);

        recarregada.Divergencia.Should().Be(-500m);
        recarregada.SaldoFinalInformado.Should().Be(69500m);
    }
}
