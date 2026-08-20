using FluentAssertions;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Produtos;
using Xunit;

namespace KiVenda.Core.Tests.Produtos;

public class ApresentacaoProdutoTests
{
    [Fact]
    public void ConverterParaUnidadeBase_Deve_Multiplicar_Pelo_Fator_De_Conversao()
    {
        // Açúcar: apresentação "1 kg" equivale a 1000 g.
        var apresentacao = new ApresentacaoProduto(Guid.NewGuid(), "1 kg", fatorConversaoParaUnidadeBase: 1000m);

        var resultado = apresentacao.ConverterParaUnidadeBase(2); // 2 sacos de 1 kg

        resultado.Should().Be(2000m);
    }

    [Fact]
    public void ConverterDeUnidadeBase_Deve_Dividir_Pelo_Fator_De_Conversao()
    {
        var apresentacao = new ApresentacaoProduto(Guid.NewGuid(), "1 kg", fatorConversaoParaUnidadeBase: 1000m);

        var resultado = apresentacao.ConverterDeUnidadeBase(2300m); // 2300 g em stock

        resultado.Should().Be(2.3m);
    }

    [Fact]
    public void Construtor_Deve_Rejeitar_Fator_De_Conversao_Nao_Positivo()
    {
        var acao = () => new ApresentacaoProduto(Guid.NewGuid(), "Inválida", fatorConversaoParaUnidadeBase: 0m);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void ConverterParaUnidadeBase_Deve_Rejeitar_Quantidade_Nao_Positiva()
    {
        var apresentacao = new ApresentacaoProduto(Guid.NewGuid(), "1 kg", fatorConversaoParaUnidadeBase: 1000m);

        var acao = () => apresentacao.ConverterParaUnidadeBase(0);

        acao.Should().Throw<DomainException>();
    }
}
