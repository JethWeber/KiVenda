using FluentAssertions;
using KiVenda.Core.Compras;
using KiVenda.Core.Enums;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using KiVenda.Core.Utilizadores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KiVenda.Persistence.Tests;

/// <summary>
/// Valida especificamente que a FK sombra "CompraId" (ItemCompra não
/// conhece o Id da Compra no domínio — ver ItemCompraConfiguration)
/// funciona corretamente: os itens sobrevivem a um round-trip completo
/// pela base de dados e continuam associados à compra certa.
/// </summary>
public class CompraPersistenceTests
{
    [Fact]
    public async Task Compra_Com_Itens_Deve_Ser_Persistida_E_Recarregada_Com_Itens_Associados()
    {
        using var fixture = new KiVendaSqliteFixture();
        var context = fixture.Context;

        var categoria = new Categoria("Mercearia");
        var unidadeGrama = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var fornecedor = new Fornecedor("Distribuidora Central", "923000000");
        var utilizador = new Utilizador("Gerente", "gerente", "hash-qualquer", PerfilUtilizador.Gerente);

        await context.Categorias.AddAsync(categoria);
        await context.UnidadesMedida.AddAsync(unidadeGrama);
        await context.Fornecedores.AddAsync(fornecedor);
        await context.Utilizadores.AddAsync(utilizador);
        await context.SaveChangesAsync();

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidadeGrama.Id, 1.5m, 5000m);
        var apresentacao25kg = produto.AdicionarApresentacao("Saco 25 kg", 25000m);
        await context.Produtos.AddAsync(produto);
        await context.SaveChangesAsync();

        var compra = new Compra(fornecedor.Id, utilizador.Id);
        compra.AdicionarItem(produto, apresentacao25kg.Id, quantidadeNaApresentacao: 2, custoTotalItem: 55000m);

        await context.Compras.AddAsync(compra);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var recarregada = await context.Compras
            .Include(c => c.Itens)
            .AsNoTracking()
            .FirstAsync(c => c.Id == compra.Id);

        recarregada.Itens.Should().ContainSingle();
        var item = recarregada.Itens.Single();
        item.QuantidadeUnidadeBase.Should().Be(50000m); // 2 sacos * 25000 g
        item.CustoUnitarioUnidadeBase.Should().Be(1.10m); // 55000 / 50000
        recarregada.CustoTotal.Should().Be(55000m);
    }
}
