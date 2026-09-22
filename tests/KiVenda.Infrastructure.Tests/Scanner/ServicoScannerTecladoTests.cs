using FluentAssertions;
using KiVenda.Infrastructure.Configuracao;
using KiVenda.Infrastructure.Scanner;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Scanner;

public sealed class ServicoScannerTecladoTests : IDisposable
{
    private readonly string _ficheiro = Path.Combine(
        Path.GetTempPath(), $"kivenda-scanner-teste-{Guid.NewGuid():N}.json");

    [Fact]
    public void Sequencia_Rapida_Terminada_Em_Enter_Deve_Disparar_Codigo()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_ficheiro);
        var scanner = new ServicoScannerTeclado(armazenamento);
        string? codigoLido = null;
        scanner.CodigoLido += codigo => codigoLido = codigo;

        foreach (var c in "5601234567890")
            scanner.ProcessarCaracter(c);

        scanner.ProcessarEnter();

        codigoLido.Should().Be("5601234567890");
    }

    [Fact]
    public async Task Sequencia_Lenta_Nao_Deve_Ser_Confundida_Com_Scanner()
    {
        var scanner = new ServicoScannerTeclado(
            new ArmazenamentoConfiguracaoLocalJson(_ficheiro));
        var disparou = false;
        scanner.CodigoLido += _ => disparou = true;

        foreach (var c in "1234")
        {
            scanner.ProcessarCaracter(c);
            await Task.Delay(70);
        }

        scanner.ProcessarEnter();

        disparou.Should().BeFalse();
    }

    [Fact]
    public void Enter_Com_Buffer_Curto_Nao_Deve_Disparar_Leitura()
    {
        var scanner = new ServicoScannerTeclado(
            new ArmazenamentoConfiguracaoLocalJson(_ficheiro));
        var disparou = false;
        scanner.CodigoLido += _ => disparou = true;

        scanner.ProcessarCaracter('1');
        scanner.ProcessarCaracter('2');
        scanner.ProcessarEnter();

        disparou.Should().BeFalse();
    }

    [Fact]
    public async Task Recarregar_Configuracao_Desativada_Deve_Impedir_Leitura()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_ficheiro);
        await armazenamento.GuardarAsync(
            ConfiguracaoScanner.Chave,
            new ConfiguracaoScanner(Ativo: false));

        var scanner = new ServicoScannerTeclado(armazenamento);
        await scanner.RecarregarConfiguracaoAsync();

        var disparou = false;
        scanner.CodigoLido += _ => disparou = true;

        foreach (var c in "123456")
            scanner.ProcessarCaracter(c);

        scanner.ProcessarEnter();

        disparou.Should().BeFalse();
        scanner.ConfiguracaoAtual.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Recarregar_Configuracao_Deve_Aplicar_Valores_Persistidos()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_ficheiro);
        var configuracao = new ConfiguracaoScanner(
            Ativo: true,
            EmitirSomAoLer: false,
            AdicionarAutomaticamente: false,
            AbrirQuantidadeAposLeitura: true);

        await armazenamento.GuardarAsync(ConfiguracaoScanner.Chave, configuracao);

        var scanner = new ServicoScannerTeclado(armazenamento);
        await scanner.RecarregarConfiguracaoAsync();

        scanner.ConfiguracaoAtual.Should().Be(configuracao);
    }

    public void Dispose()
    {
        if (File.Exists(_ficheiro))
            File.Delete(_ficheiro);
    }
}
