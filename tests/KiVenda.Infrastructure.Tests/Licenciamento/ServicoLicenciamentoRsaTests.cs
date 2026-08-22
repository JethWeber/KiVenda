using FluentAssertions;
using KiVenda.Infrastructure.Licenciamento;
using Xunit;

namespace KiVenda.Infrastructure.Tests.Licenciamento;

public class ServicoLicenciamentoRsaTests : IDisposable
{
    private readonly string _pastaTeste;
    private readonly string _caminhoLicencaAtiva;

    public ServicoLicenciamentoRsaTests()
    {
        _pastaTeste = Path.Combine(Path.GetTempPath(), $"kivenda-licenca-teste-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_pastaTeste);
        _caminhoLicencaAtiva = Path.Combine(_pastaTeste, "licenca.wta");
    }

    [Fact]
    public async Task AtivarLicencaAsync_Com_Licenca_Assinada_Corretamente_Deve_Ser_Aceite()
    {
        using var chaves = FerramentasLicencaDeTeste.CriarParDeChaves();
        var conteudoLicenca = FerramentasLicencaDeTeste.AssinarLicencaDeTeste(chaves, "Cantina da Maria");
        var caminhoOrigem = Path.Combine(_pastaTeste, "nova-licenca.wta");
        await File.WriteAllTextAsync(caminhoOrigem, conteudoLicenca);

        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chaves);
        var resultado = await servico.AtivarLicencaAsync(caminhoOrigem);

        resultado.Valida.Should().BeTrue();
        resultado.NomeCliente.Should().Be("Cantina da Maria");
        File.Exists(_caminhoLicencaAtiva).Should().BeTrue();
    }

    [Fact]
    public async Task ValidarLicencaAtualAsync_Sem_Nenhuma_Licenca_Ativada_Deve_Reprovar()
    {
        using var chaves = FerramentasLicencaDeTeste.CriarParDeChaves();
        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chaves);

        var resultado = await servico.ValidarLicencaAtualAsync();

        resultado.Valida.Should().BeFalse();
        resultado.MensagemErro.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ValidarLicencaAtualAsync_Deve_Reprovar_Licenca_Assinada_Com_Outra_Chave()
    {
        using var chavesReais = FerramentasLicencaDeTeste.CriarParDeChaves();
        using var chavesDeOutraLicenca = FerramentasLicencaDeTeste.CriarParDeChaves();

        // Licença "forjada": assinada com uma chave diferente da que o
        // serviço vai usar para verificar.
        var conteudoForjado = FerramentasLicencaDeTeste.AssinarLicencaDeTeste(chavesDeOutraLicenca, "Loja Pirata");
        await File.WriteAllTextAsync(_caminhoLicencaAtiva, conteudoForjado);

        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chavesReais);
        var resultado = await servico.ValidarLicencaAtualAsync();

        resultado.Valida.Should().BeFalse();
        resultado.MensagemErro.Should().Contain("Assinatura");
    }

    [Fact]
    public async Task ValidarLicencaAtualAsync_Deve_Reprovar_Licenca_Expirada()
    {
        using var chaves = FerramentasLicencaDeTeste.CriarParDeChaves();
        var conteudo = FerramentasLicencaDeTeste.AssinarLicencaDeTeste(chaves, "Cantina da Maria", dataExpiracao: DateTime.UtcNow.AddDays(-1));
        await File.WriteAllTextAsync(_caminhoLicencaAtiva, conteudo);

        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chaves);
        var resultado = await servico.ValidarLicencaAtualAsync();

        resultado.Valida.Should().BeFalse();
        resultado.MensagemErro.Should().Contain("expirada");
    }

    [Fact]
    public async Task ValidarLicencaAtualAsync_Deve_Aceitar_Licenca_Sem_Data_De_Expiracao()
    {
        using var chaves = FerramentasLicencaDeTeste.CriarParDeChaves();
        var conteudo = FerramentasLicencaDeTeste.AssinarLicencaDeTeste(chaves, "Cantina da Maria", dataExpiracao: null);
        await File.WriteAllTextAsync(_caminhoLicencaAtiva, conteudo);

        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chaves);
        var resultado = await servico.ValidarLicencaAtualAsync();

        resultado.Valida.Should().BeTrue();
    }

    [Fact]
    public async Task ValidarLicencaAtualAsync_Deve_Reprovar_Ficheiro_Corrompido()
    {
        using var chaves = FerramentasLicencaDeTeste.CriarParDeChaves();
        await File.WriteAllTextAsync(_caminhoLicencaAtiva, "isto não é um envelope de licença válido");

        var servico = new ServicoLicenciamentoRsa(_caminhoLicencaAtiva, chaves);
        var resultado = await servico.ValidarLicencaAtualAsync();

        resultado.Valida.Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaTeste))
        {
            Directory.Delete(_pastaTeste, recursive: true);
        }
    }
}
