using FluentAssertions;
using KiVenda.Core.Produtos;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

public class ProdutoPersistenceTests
{
    [Fact]
    public async Task Produto_Com_Multiplas_Apresentacoes_Deve_Ser_Persistido_E_Recarregado_Corretamente()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidadeGrama = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidadeGrama);
        await context.SaveChangesAsync();

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidadeGrama.Id, precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);
        produto.AdicionarApresentacao("250 g", 250m);
        produto.AdicionarApresentacao("1 kg", 1000m, codigoBarras: "7891111111111");
        produto.AdicionarApresentacao("25 kg", 25000m);

        await context.Produtos.AddAsync(produto);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregado = await context.Produtos
            .Include(p => p.Apresentacoes)
            .AsNoTracking()
            .FirstAsync(p => p.Id == produto.Id);

        // "Unidade base" (fator 1, criada automaticamente) + 3 apresentações adicionadas.
        recarregado.Apresentacoes.Should().HaveCount(4);
        recarregado.Apresentacoes.Should().Contain(a => a.Nome == "1 kg" && a.CodigoBarras == "7891111111111");
        recarregado.Apresentacoes.Single(a => a.Nome == "25 kg").FatorConversaoParaUnidadeBase.Should().Be(25000m);
    }

    [Fact]
    public async Task CodigoInterno_Deve_Ser_Unico()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Unidade, "Unidade");
        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidade);
        await context.SaveChangesAsync();

        await context.Produtos.AddAsync(new Produto("Produto A", "PRD-001", categoria.Id, unidade.Id, 100m, 1m));
        await context.SaveChangesAsync();

        await context.Produtos.AddAsync(new Produto("Produto B", "PRD-001", categoria.Id, unidade.Id, 200m, 1m));

        var acao = async () => await context.SaveChangesAsync();

        await acao.Should().ThrowAsync<DbUpdateException>();
    }
}
