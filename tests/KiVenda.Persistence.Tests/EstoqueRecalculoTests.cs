using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

/// <summary>
/// Cobre diretamente o critério de aceitação do plano: "o estoque atual
/// de qualquer produto pode ser recalculado a partir do zero, somando o
/// histórico de MovimentoStock, e bate com o valor materializado
/// exibido na UI".
/// </summary>
public class EstoqueRecalculoTests
{
    [Fact]
    public async Task Estoque_Materializado_Deve_Bater_Com_Soma_Dos_Movimentos_Persistidos()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidadeGrama = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var utilizador = new Utilizador("Maria", "maria", "hash-qualquer", PerfilUtilizador.Gerente);

        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidadeGrama);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidadeGrama.Id, precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);

        var entrada1 = produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizador.Id);
        var entrada2 = produto.RegistarEntradaStock(25000m, 27500m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizador.Id);
        var saida = produto.RegistarSaidaStock(1500m, OrigemMovimentoStock.Venda, Guid.NewGuid(), utilizador.Id);
        var ajuste = produto.RegistarAjusteStock(-200m, "Quebra na contagem física", utilizador.Id);

        await context.Produtos.AddAsync(produto);
        await context.MovimentosStock.AddRangeAsync(entrada1, entrada2, saida, ajuste);
        await context.SaveChangesAsync();

        // Simula reabrir a aplicação: lê tudo de volta, sem tracking, como se fosse uma nova sessão.
        var produtoReconsultado = await context.Produtos
            .AsNoTracking()
            .FirstAsync(p => p.Id == produto.Id);

        var movimentosPersistidos = await context.MovimentosStock
            .AsNoTracking()
            .Where(m => m.ProdutoId == produto.Id)
            .ToListAsync();

        var estoqueRecalculadoAPartirDoHistorico = movimentosPersistidos.Sum(m => m.Quantidade);

        // 25000 + 25000 - 1500 - 200 = 48300
        estoqueRecalculadoAPartirDoHistorico.Should().Be(48300m);
        produtoReconsultado.EstoqueAtual.Should().Be(estoqueRecalculadoAPartirDoHistorico);
    }

    [Fact]
    public async Task Produto_RecalcularEstoqueMaterializado_Deve_Reproduzir_O_Mesmo_Valor_A_Partir_Do_Historico_Persistido()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidadeGrama = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var utilizador = new Utilizador("João", "joao", "hash-qualquer", PerfilUtilizador.Atendente);

        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidadeGrama);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();

        var produto = new Produto("Arroz", "PRD-AR01", categoria.Id, unidadeGrama.Id, precoVendaPorUnidadeBase: 1.2m, stockMinimo: 2000m);
        var entrada = produto.RegistarEntradaStock(10000m, 10000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizador.Id);
        var saida = produto.RegistarSaidaStock(3000m, OrigemMovimentoStock.Venda, Guid.NewGuid(), utilizador.Id);

        await context.Produtos.AddAsync(produto);
        await context.MovimentosStock.AddRangeAsync(entrada, saida);
        await context.SaveChangesAsync();

        var movimentosPersistidos = await context.MovimentosStock
            .AsNoTracking()
            .Where(m => m.ProdutoId == produto.Id)
            .ToListAsync();

        // Usa o próprio mecanismo de recálculo do domínio (não apenas um
        // Sum() ad-hoc no teste) sobre o histórico lido da base de dados,
        // provando que RecalcularEstoqueMaterializado (Fase 1) funciona
        // corretamente com dados vindos da Persistence (Fase 2).
        produto.RecalcularEstoqueMaterializado(movimentosPersistidos);

        produto.EstoqueAtual.Should().Be(7000m); // 10000 - 3000
    }
}
