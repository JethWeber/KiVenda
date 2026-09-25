using System.Text;
using FluentAssertions;
using KiVenda.Application.Vendas;
using KiVenda.Core.Enums;
using KiVenda.Infrastructure.Impressao;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Impressao;

public sealed class GeradorEscPosTests
{
    [Fact]
    public void Deve_gerar_comandos_ESC_POS_para_recibo()
    {
        var configuracao = new ConfiguracaoImpressoraTermica(
            "/dev/usb/lp0",
            Colunas: 48,
            CortarPapel: true,
            EncodingNome: "ascii");

        var gerador = new GeradorEscPos(configuracao);

        var recibo = new ReciboVendaDto(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            new DateTime(2026, 9, 25, 18, 30, 0, DateTimeKind.Local),
            [
                new ItemReciboDto("Arroz", "Un", 2, 1000m)
            ],
            1000m,
            1000m,
            200m,
            MetodoPagamento.Dinheiro,
            "Jeth");

        var dadosLoja = new DadosLoja(
            "Cantina Modelo",
            Nif: "123456789",
            Endereco: "Luanda");

        var dados = gerador.GerarRecibo(recibo, dadosLoja);

        dados.Should().ContainInOrder((byte)0x1B, (byte)0x40);
        Encoding.ASCII.GetString(dados).Should().Contain("Cantina Modelo");
        Encoding.ASCII.GetString(dados).Should().Contain("TOTAL A PAGAR");
        dados.Should().ContainInOrder((byte)0x1D, (byte)0x56, (byte)0x00);
    }

    [Fact]
    public void Deve_gerar_sem_corte_quando_desativado()
    {
        var configuracao = new ConfiguracaoImpressoraTermica(
            "/dev/usb/lp0",
            CortarPapel: false,
            EncodingNome: "ascii");

        var gerador = new GeradorEscPos(configuracao);
        var dados = gerador.GerarTeste("TESTE");

        dados.Should().NotContainInOrder((byte)0x1D, (byte)0x56, (byte)0x00);
    }
}
