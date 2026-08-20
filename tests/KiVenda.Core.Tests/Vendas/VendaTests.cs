using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using KiVenda.Core.Vendas;
using Xunit;

namespace KiVenda.Core.Tests.Vendas;

public class VendaTests
{
    private static Produto CriarAcucarComEstoque(out ApresentacaoProduto apresentacao1kg, decimal estoqueInicialG = 25000m)
    {
        var produto = new Produto("Açúcar", "PRD-AC01", Guid.NewGuid(), Guid.NewGuid(), precoVendaPorUnidadeBase: 1.5m, stockMinimo: 5000m);
        apresentacao1kg = produto.AdicionarApresentacao("1 kg", fatorConversaoParaUnidadeBase: 1000m);

        produto.RegistarEntradaStock(estoqueInicialG, estoqueInicialG, OrigemMovimentoStock.Compra, Guid.NewGuid(), Guid.NewGuid());

        return produto;
    }

    [Fact]
    public void AdicionarItem_Deve_Converter_Apresentacao_Para_Unidade_Base_E_Fotografar_Preco_E_Custo()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());

        var item = venda.AdicionarItem(produto, apresentacao1kg.Id, quantidadeNaApresentacao: 2); // 2 kg

        item.QuantidadeUnidadeBase.Should().Be(2000m);
        item.PrecoUnitarioUnidadeBase.Should().Be(1.5m);
        item.CustoUnitarioUnidadeBase.Should().Be(1m); // custo médio ponderado após 1 única entrada
        item.ValorTotal.Should().Be(3000m);
    }

    [Fact]
    public void Total_Deve_Descontar_O_Valor_Aplicado_Sem_Ficar_Negativo()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());
        venda.AdicionarItem(produto, apresentacao1kg.Id, 2); // subtotal 3000

        venda.AplicarDesconto(500m);

        venda.Total.Should().Be(2500m);
    }

    [Fact]
    public void AplicarDesconto_Deve_Rejeitar_Valor_Maior_Que_O_Subtotal()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());
        venda.AdicionarItem(produto, apresentacao1kg.Id, 1); // subtotal 1500

        var acao = () => venda.AplicarDesconto(2000m);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Finalizar_Deve_Rejeitar_Venda_Sem_Itens()
    {
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());

        var acao = () => venda.Finalizar();

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Finalizar_Deve_Rejeitar_Pagamento_Insuficiente()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());
        venda.AdicionarItem(produto, apresentacao1kg.Id, 2); // total 3000
        venda.AdicionarPagamento(MetodoPagamento.Dinheiro, 2000m);

        var acao = () => venda.Finalizar();

        acao.Should().Throw<DomainException>().WithMessage("*Pagamento insuficiente*");
    }

    [Fact]
    public void Finalizar_Deve_Aceitar_Pagamento_Misto_Que_Cubra_O_Total()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());
        venda.AdicionarItem(produto, apresentacao1kg.Id, 2); // total 3000
        venda.AdicionarPagamento(MetodoPagamento.Dinheiro, 1000m);
        venda.AdicionarPagamento(MetodoPagamento.Tpa, 2000m);

        venda.Finalizar();

        venda.Estado.Should().Be(EstadoVenda.Finalizada);
    }

    [Fact]
    public void AdicionarItem_Depois_De_Finalizada_Deve_Ser_Rejeitado()
    {
        var produto = CriarAcucarComEstoque(out var apresentacao1kg);
        var venda = new Venda(Guid.NewGuid(), Guid.NewGuid());
        venda.AdicionarItem(produto, apresentacao1kg.Id, 2);
        venda.AdicionarPagamento(MetodoPagamento.Dinheiro, 3000m);
        venda.Finalizar();

        var acao = () => venda.AdicionarItem(produto, apresentacao1kg.Id, 1);

        acao.Should().Throw<DomainException>();
    }
}
