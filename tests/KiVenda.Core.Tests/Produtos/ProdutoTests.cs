using FluentAssertions;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Core.Tests.Produtos;

public class ProdutoTests
{
    private static Produto CriarAcucar(decimal precoVenda = 1.5m, decimal stockMinimo = 5000m)
    {
        return new Produto(
            nome: "Açúcar",
            codigoInterno: "PRD-AC01",
            categoriaId: Guid.NewGuid(),
            unidadeBaseId: Guid.NewGuid(),
            precoVendaPorUnidadeBase: precoVenda,
            stockMinimo: stockMinimo);
    }

    [Fact]
    public void Produto_Deve_Nascer_Com_Uma_Apresentacao_Padrao_De_Fator_Um()
    {
        var produto = CriarAcucar();

        produto.Apresentacoes.Should().ContainSingle();
        produto.Apresentacoes.Single().FatorConversaoParaUnidadeBase.Should().Be(1m);
    }

    [Fact]
    public void RegistarEntradaStock_Deve_Aumentar_O_Estoque_Atual()
    {
        var produto = CriarAcucar();

        produto.RegistarEntradaStock(25000m, custoTotal: 25000m, OrigemMovimentoStock.Compra, origemId: Guid.NewGuid(), utilizadorId: Guid.NewGuid());

        produto.EstoqueAtual.Should().Be(25000m);
    }

    [Fact]
    public void RegistarEntradaStock_Deve_Calcular_Custo_Medio_Ponderado_Entre_Compras_Sucessivas()
    {
        // Compra 1: 25.000 g a 25.000 Kz -> 1 Kz/g
        // Compra 2: 25.000 g a 27.500 Kz -> 1,10 Kz/g
        // Custo médio ponderado esperado: (25000*1 + 25000*1.10) / 50000 = 1,05 Kz/g
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();

        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);
        produto.RegistarEntradaStock(25000m, 27500m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        produto.EstoqueAtual.Should().Be(50000m);
        produto.CustoMedioPonderado.Should().Be(1.05m);
    }

    [Fact]
    public void RegistarSaidaStock_Deve_Diminuir_O_Estoque_Atual()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        produto.RegistarSaidaStock(1500m, OrigemMovimentoStock.Venda, Guid.NewGuid(), utilizadorId);

        produto.EstoqueAtual.Should().Be(23500m);
    }

    [Fact]
    public void RegistarSaidaStock_Deve_Rejeitar_Quantidade_Maior_Que_O_Estoque_Disponivel()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(1000m, 1000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        var acao = () => produto.RegistarSaidaStock(1500m, OrigemMovimentoStock.Venda, Guid.NewGuid(), utilizadorId);

        acao.Should().Throw<DomainException>().WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public void RegistarAjusteStock_Deve_Aceitar_Delta_Negativo_Ate_Ao_Limite_Do_Estoque()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(1000m, 1000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        produto.RegistarAjusteStock(-200m, "Quebra por embalagem danificada", utilizadorId);

        produto.EstoqueAtual.Should().Be(800m);
    }

    [Fact]
    public void RegistarAjusteStock_Deve_Rejeitar_Delta_Que_Deixe_O_Estoque_Negativo()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(100m, 100m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        var acao = () => produto.RegistarAjusteStock(-200m, "Contagem física", utilizadorId);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistarAjusteStock_Deve_Exigir_Motivo()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(100m, 100m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        var acao = () => produto.RegistarAjusteStock(-10m, "", utilizadorId);

        acao.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0, EstadoStock.SemStock)]
    [InlineData(5000, EstadoStock.StockBaixo)]
    [InlineData(5001, EstadoStock.EmStock)]
    public void ObterEstadoStock_Deve_Refletir_O_Stock_Minimo(decimal estoqueAtual, EstadoStock esperado)
    {
        var produto = CriarAcucar(stockMinimo: 5000m);
        var utilizadorId = Guid.NewGuid();

        if (estoqueAtual > 0)
        {
            produto.RegistarEntradaStock(estoqueAtual, estoqueAtual, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);
        }

        produto.ObterEstadoStock().Should().Be(esperado);
    }

    [Fact]
    public void CalcularLucroEstimado_Deve_Usar_Custo_Medio_Ponderado_Nao_Preco_Fixo()
    {
        // Preço de venda: 1,50 Kz/g. Custo médio ponderado após as duas
        // compras do teste anterior: 1,05 Kz/g. Lucro esperado por grama: 0,45 Kz.
        var produto = CriarAcucar(precoVenda: 1.5m);
        var utilizadorId = Guid.NewGuid();
        produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);
        produto.RegistarEntradaStock(25000m, 27500m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);

        var lucro = produto.CalcularLucroEstimado(500m); // venda de 500 g

        lucro.Should().Be(225m); // 500 * (1.5 - 1.05)
    }

    [Fact]
    public void RecalcularEstoqueMaterializado_Deve_Bater_Com_A_Soma_Dos_Movimentos()
    {
        var produto = CriarAcucar();
        var utilizadorId = Guid.NewGuid();

        var entrada = produto.RegistarEntradaStock(25000m, 25000m, OrigemMovimentoStock.Compra, Guid.NewGuid(), utilizadorId);
        var saida = produto.RegistarSaidaStock(1500m, OrigemMovimentoStock.Venda, Guid.NewGuid(), utilizadorId);
        var ajuste = produto.RegistarAjusteStock(-100m, "Quebra", utilizadorId);

        produto.RecalcularEstoqueMaterializado(new[] { entrada, saida, ajuste });

        produto.EstoqueAtual.Should().Be(23400m); // 25000 - 1500 - 100
    }

    [Fact]
    public void AdicionarApresentacao_Deve_Rejeitar_Nome_Duplicado_Entre_Apresentacoes_Ativas()
    {
        var produto = CriarAcucar();
        produto.AdicionarApresentacao("1 kg", 1000m);

        var acao = () => produto.AdicionarApresentacao("1 kg", 1000m);

        acao.Should().Throw<DomainException>();
    }
}
