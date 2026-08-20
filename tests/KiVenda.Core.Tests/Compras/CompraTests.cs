using FluentAssertions;
using KiVenda.Core.Compras;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Core.Tests.Compras;

public class CompraTests
{
    private static Produto CriarAcucarComApresentacaoDe25Kg(out ApresentacaoProduto apresentacao25kg)
    {
        var produto = new Produto("Açúcar", "PRD-AC01", Guid.NewGuid(), Guid.NewGuid(), precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);
        apresentacao25kg = produto.AdicionarApresentacao("Saco 25 kg", fatorConversaoParaUnidadeBase: 25000m);

        return produto;
    }

    [Fact]
    public void AdicionarItem_Deve_Converter_Para_Unidade_Base_E_Calcular_Custo_Por_Unidade()
    {
        var produto = CriarAcucarComApresentacaoDe25Kg(out var apresentacao25kg);
        var compra = new Compra(Guid.NewGuid(), Guid.NewGuid());

        var item = compra.AdicionarItem(produto, apresentacao25kg.Id, quantidadeNaApresentacao: 1, custoTotalItem: 27500m);

        item.QuantidadeUnidadeBase.Should().Be(25000m);
        item.CustoUnitarioUnidadeBase.Should().Be(1.10m);
        compra.CustoTotal.Should().Be(27500m);
    }

    [Fact]
    public void AdicionarItem_Deve_Rejeitar_Apresentacao_De_Outro_Produto()
    {
        var produtoA = CriarAcucarComApresentacaoDe25Kg(out _);
        var produtoB = CriarAcucarComApresentacaoDe25Kg(out var apresentacaoDeB);
        var compra = new Compra(Guid.NewGuid(), Guid.NewGuid());

        var acao = () => compra.AdicionarItem(produtoA, apresentacaoDeB.Id, 1, 27500m);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void AdicionarItem_Deve_Rejeitar_Quantidade_Nao_Positiva()
    {
        var produto = CriarAcucarComApresentacaoDe25Kg(out var apresentacao25kg);
        var compra = new Compra(Guid.NewGuid(), Guid.NewGuid());

        var acao = () => compra.AdicionarItem(produto, apresentacao25kg.Id, 0, 27500m);

        acao.Should().Throw<DomainException>();
    }
}
