using FluentAssertions;
using KiVenda.Infrastructure.Configuracao;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Configuracao;

public class ArmazenamentoConfiguracaoLocalJsonTests : IDisposable
{
    private readonly string _caminhoFicheiro;

    public ArmazenamentoConfiguracaoLocalJsonTests()
    {
        _caminhoFicheiro = Path.Combine(Path.GetTempPath(), $"kivenda-config-teste-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public async Task Guardar_E_Obter_Deve_Devolver_O_Mesmo_Valor()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro);
        var configuracao = new ConfiguracaoScanner(Ativo: true, EmitirSomAoLer: false, AdicionarAutomaticamente: true, AbrirQuantidadeAposLeitura: true);

        await armazenamento.GuardarAsync(ConfiguracaoScanner.Chave, configuracao);
        var lido = await armazenamento.ObterAsync<ConfiguracaoScanner>(ConfiguracaoScanner.Chave);

        lido.Should().Be(configuracao);
    }

    [Fact]
    public async Task Obter_Chave_Inexistente_Deve_Devolver_Default()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro);

        var lido = await armazenamento.ObterAsync<ConfiguracaoScanner>("chave-que-nao-existe");

        lido.Should().BeNull();
    }

    [Fact]
    public async Task Guardar_Deve_Persistir_No_Disco_E_Sobreviver_A_Nova_Instancia()
    {
        var configuracao = new ConfiguracaoScanner(EmitirSomAoLer: false);
        await new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro).GuardarAsync(ConfiguracaoScanner.Chave, configuracao);

        // Nova instância, simula reabrir a aplicação.
        var novaInstancia = new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro);
        var lido = await novaInstancia.ObterAsync<ConfiguracaoScanner>(ConfiguracaoScanner.Chave);

        lido.Should().Be(configuracao);
    }

    [Fact]
    public async Task Guardar_Duas_Chaves_Diferentes_Nao_Deve_Perder_Nenhuma()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro);

        await armazenamento.GuardarAsync("tema", "escuro");
        await armazenamento.GuardarAsync("idioma", "pt-AO");

        (await armazenamento.ObterAsync<string>("tema")).Should().Be("escuro");
        (await armazenamento.ObterAsync<string>("idioma")).Should().Be("pt-AO");
    }

    [Fact]
    public async Task Remover_Deve_Apagar_Apenas_A_Chave_Indicada()
    {
        var armazenamento = new ArmazenamentoConfiguracaoLocalJson(_caminhoFicheiro);
        await armazenamento.GuardarAsync("tema", "escuro");
        await armazenamento.GuardarAsync("idioma", "pt-AO");

        await armazenamento.RemoverAsync("tema");

        (await armazenamento.ObterAsync<string>("tema")).Should().BeNull();
        (await armazenamento.ObterAsync<string>("idioma")).Should().Be("pt-AO");
    }

    public void Dispose()
    {
        if (File.Exists(_caminhoFicheiro))
        {
            File.Delete(_caminhoFicheiro);
        }
    }
}
