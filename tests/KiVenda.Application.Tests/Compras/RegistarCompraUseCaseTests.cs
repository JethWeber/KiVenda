using FluentAssertions;
using KiVenda.Application.Compras;
using KiVenda.Application.Exceptions;
using KiVenda.Application.Tests.Fakes;
using KiVenda.Core.Enums;
using KiVenda.Core.Fornecedores;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Application.Tests.Compras;

public class RegistarCompraUseCaseTests
{
    [Fact]
    public async Task RegistarCompra_Deve_Dar_Entrada_De_Stock_E_Atualizar_Custo_Medio_Ponderado()
    {
        var db = new InMemoryDatabase();
        var categoria = new Categoria("Mercearia");
        var unidade = new UnidadeMedida(UnidadeMedida.Padrao.Grama, "Grama");
        var fornecedor = new Fornecedor("Distribuidora Central");
        db.Categorias.Add(categoria);
        db.UnidadesMedida.Add(unidade);
        db.Fornecedores.Add(fornecedor);

        var produto = new Produto("Açúcar", "PRD-AC01", categoria.Id, unidade.Id, 1.5m, 5000m);
        var apresentacao25kg = produto.AdicionarApresentacao("Saco 25 kg", 25000m);
        // Compra anterior já registada: 25000 g a 1 Kz/g.
        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), Guid.NewGuid());
        db.Produtos.Add(produto);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Gerente };
        var uow = new InMemoryUnitOfWork(db);
        var useCase = new RegistarCompraUseCase(uow, contexto);

        var comando = new RegistarCompraCommand(
            fornecedor.Id,
            new[] { new ItemCompraCommand(produto.Id, apresentacao25kg.Id, QuantidadeNaApresentacao: 1, CustoTotalItem: 27500m) });

        var compraId = await useCase.ExecutarAsync(comando);

        db.Compras.Should().ContainSingle(c => c.Id == compraId);
        produto.EstoqueAtual.Should().Be(50000m); // 25000 + 25000
        produto.CustoMedioPonderado.Should().Be(1.05m); // (25000*1 + 25000*1.10) / 50000
        db.MovimentosStock.Should().ContainSingle(m => m.Tipo == TipoMovimentoStock.Entrada && m.OrigemId == compraId);
    }

    [Fact]
    public async Task Atendente_Nao_Deve_Conseguir_Registar_Compra()
    {
        var db = new InMemoryDatabase();
        var fornecedor = new Fornecedor("Distribuidora Central");
        db.Fornecedores.Add(fornecedor);

        var contexto = new FakeContextoAutenticacao { Perfil = PerfilUtilizador.Atendente };
        var useCase = new RegistarCompraUseCase(new InMemoryUnitOfWork(db), contexto);

        var acao = async () => await useCase.ExecutarAsync(new RegistarCompraCommand(fornecedor.Id, Array.Empty<ItemCompraCommand>()));

        await acao.Should().ThrowAsync<PermissaoNegadaException>();
    }
}
