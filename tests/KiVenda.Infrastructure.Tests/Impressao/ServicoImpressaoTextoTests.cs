using FluentAssertions;
using KiVenda.Application.Vendas;
using KiVenda.Core.Enums;
using KiVenda.Infrastructure.Impressao;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Impressao;

public class ServicoImpressaoTextoTests : IDisposable
{
    private readonly string _pastaRecibos;

    public ServicoImpressaoTextoTests()
    {
        _pastaRecibos = Path.Combine(Path.GetTempPath(), $"kivenda-recibos-teste-{Guid.NewGuid():N}");
    }

    [Fact]
    public async Task ImprimirReciboVendaAsync_Deve_Gerar_Ficheiro_Com_Os_Dados_Da_Venda()
    {
        var servico = new ServicoImpressaoTexto(_pastaRecibos);
        var dadosLoja = new DadosLoja("Cantina da Maria", "Rua Principal, Luanda-Sul", "923 000 000");

        var recibo = new ReciboVendaDto(
            VendaId: Guid.NewGuid(),
            Data: new DateTime(2026, 8, 22, 14, 30, 0),
            Itens: new[] { new ItemReciboDto("Açúcar", "1 kg", 2m, 3000m) },
            Subtotal: 3000m,
            Total: 3000m,
            LucroEstimado: 1000m,
            MetodoPagamento: MetodoPagamento.Dinheiro,
            OperadorNome: "Maria Silva");

        await servico.ImprimirReciboVendaAsync(recibo, dadosLoja);

        var ficheiros = Directory.GetFiles(_pastaRecibos, "recibo-*.txt");
        ficheiros.Should().ContainSingle();

        var conteudo = await File.ReadAllTextAsync(ficheiros[0]);
        conteudo.Should().Contain("Cantina da Maria");
        conteudo.Should().Contain("Açúcar");
        conteudo.Should().Contain("1 kg");
        conteudo.Should().Contain("3.000,00 Kz");
        conteudo.Should().Contain("Dinheiro");
        conteudo.Should().NotContain("Desconto");
        conteudo.Should().NotContain("Valor pago");
        conteudo.Should().NotContain("Troco");
    }

    [Fact]
    public async Task ImprimirReciboVendaAsync_Deve_Respeitar_Total_Sem_Desconto()
    {
        var servico = new ServicoImpressaoTexto(_pastaRecibos);
        var dadosLoja = new DadosLoja("Cantina da Maria");

        var recibo = new ReciboVendaDto(
            Guid.NewGuid(),
            DateTime.Now,
            new[] { new ItemReciboDto("Arroz", "5 kg", 1m, 5000m) },
            Subtotal: 5000m,
            Total: 5000m,
            LucroEstimado: 800m,
            MetodoPagamento: MetodoPagamento.Multicaixa,
            OperadorNome: "João Pedro");

        await servico.ImprimirReciboVendaAsync(recibo, dadosLoja);

        var ficheiro = Directory.GetFiles(_pastaRecibos, "recibo-*.txt").Single();
        var conteudo = await File.ReadAllTextAsync(ficheiro);

        conteudo.Should().Contain("5.000,00 Kz");
        conteudo.Should().NotContain("Desconto");
    }

    [Fact]
    public async Task ImprimirTextoAsync_Deve_Gravar_Conteudo_Livre_Para_Relatorios()
    {
        var servico = new ServicoImpressaoTexto(_pastaRecibos);

        await servico.ImprimirTextoAsync("Relatório Diário", "Total vendido: 150.400,00 Kz");

        var ficheiros = Directory.GetFiles(_pastaRecibos, "Relatório*.txt");
        ficheiros.Should().ContainSingle();
        (await File.ReadAllTextAsync(ficheiros[0])).Should().Contain("150.400,00 Kz");
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaRecibos))
        {
            Directory.Delete(_pastaRecibos, recursive: true);
        }
    }
}