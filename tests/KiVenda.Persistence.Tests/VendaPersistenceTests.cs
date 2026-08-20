using FluentAssertions;
using KiVenda.Core.Caixa;
using KiVenda.Core.Enums;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using KiVenda.Core.Vendas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

public class VendaPersistenceTests
{
    [Fact]
    public async Task Venda_Com_Itens_E_Pagamento_Misto_Deve_Ser_Persistida_E_Recarregada_Corretamente()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidadeGrama = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var utilizador = new Utilizador("João", "joao", "hash-qualquer", PerfilUtilizador.Atendente);
        var sessao = new SessaoCaixa(utilizador.Id, saldoInicial: 20000m);

        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidadeGrama);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SessoesCaixa.AddAsync(sessao);
        await context.SaveChangesAsync();

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidadeGrama.Id, precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);
        var apresentacao1kg = produto.AdicionarApresentacao("1 kg", 1000m);
        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizador.Id);
        await context.Produtos.AddAsync(produto);
        await context.SaveChangesAsync();

        var venda = new Venda(utilizador.Id, sessao.Id);
        venda.AdicionarItem(produto, apresentacao1kg.Id, quantidadeNaApresentacao: 2); // total 3000
        venda.AdicionarPagamento(MetodoPagamento.Dinheiro, 1000m);
        venda.AdicionarPagamento(MetodoPagamento.Tpa, 2000m);
        venda.Finalizar();

        await context.Vendas.AddAsync(venda);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregada = await context.Vendas
            .Include(v => v.Itens)
            .Include(v => v.Pagamentos)
            .AsNoTracking()
            .FirstAsync(v => v.Id == venda.Id);

        recarregada.Itens.Should().ContainSingle();
        recarregada.Itens.Single().QuantidadeUnidadeBase.Should().Be(2000m);
        recarregada.Pagamentos.Should().HaveCount(2);
        recarregada.Pagamentos.Sum(p => p.Valor).Should().Be(3000m);
        recarregada.Estado.Should().Be(EstadoVenda.Finalizada);
    }
}
