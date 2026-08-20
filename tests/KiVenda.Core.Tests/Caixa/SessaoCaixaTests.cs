using FluentAssertions;
using KiVenda.Core.Caixa;
using KiVenda.Core.Exceptions;
using Xunit;

namespace KiVenda.Core.Tests.Caixa;

public class SessaoCaixaTests
{
    [Fact]
    public void SaldoCalculado_Deve_Somar_Entradas_E_Subtrair_Saidas_Ao_Saldo_Inicial()
    {
        var utilizadorId = Guid.NewGuid();
        var sessao = new SessaoCaixa(utilizadorId, saldoInicial: 20000m);

        sessao.RegistarSuprimento(50000m, utilizadorId);
        sessao.RegistarEntradaDeVenda(12500m, utilizadorId, Guid.NewGuid());
        sessao.RegistarSangria(35000m, utilizadorId);

        // 20000 + 50000 + 12500 - 35000 = 47500
        sessao.SaldoCalculado.Should().Be(47500m);
    }

    [Fact]
    public void RegistarSangria_Deve_Rejeitar_Valor_Maior_Que_O_Saldo_Atual()
    {
        var utilizadorId = Guid.NewGuid();
        var sessao = new SessaoCaixa(utilizadorId, saldoInicial: 10000m);

        var acao = () => sessao.RegistarSangria(20000m, utilizadorId);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Fechar_Deve_Calcular_Divergencia_Positiva_Quando_Ha_Sobra()
    {
        var utilizadorId = Guid.NewGuid();
        var sessao = new SessaoCaixa(utilizadorId, saldoInicial: 20000m);
        sessao.RegistarSuprimento(50000m, utilizadorId); // saldo calculado: 70000

        var divergencia = sessao.Fechar(saldoInformado: 70500m, utilizadorId);

        divergencia.Should().Be(500m);
    }

    [Fact]
    public void Fechar_Deve_Calcular_Divergencia_Negativa_Quando_Ha_Falta()
    {
        var utilizadorId = Guid.NewGuid();
        var sessao = new SessaoCaixa(utilizadorId, saldoInicial: 20000m);
        sessao.RegistarSuprimento(50000m, utilizadorId); // saldo calculado: 70000

        var divergencia = sessao.Fechar(saldoInformado: 69200m, utilizadorId);

        divergencia.Should().Be(-800m);
    }

    [Fact]
    public void Operacoes_Depois_De_Fechada_Devem_Ser_Rejeitadas()
    {
        var utilizadorId = Guid.NewGuid();
        var sessao = new SessaoCaixa(utilizadorId, saldoInicial: 10000m);
        sessao.Fechar(10000m, utilizadorId);

        var acao = () => sessao.RegistarSuprimento(1000m, utilizadorId);

        acao.Should().Throw<DomainException>();
    }
}
