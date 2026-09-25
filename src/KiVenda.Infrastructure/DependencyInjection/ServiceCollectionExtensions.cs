using System.Security.Cryptography;
using KiVenda.Application.Abstractions.Auth;
using KiVenda.Infrastructure.Autenticacao;
using KiVenda.Infrastructure.Backup;
using KiVenda.Infrastructure.Caminhos;
using KiVenda.Infrastructure.Configuracao;
using KiVenda.Infrastructure.Impressao;
using KiVenda.Infrastructure.Licenciamento;
using KiVenda.Infrastructure.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Infrastructure.DependencyInjection;

/// <summary>
/// Ponto único de registo desta camada no composition root do Desktop
/// (Fase 6).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="services">Coleção de serviços do composition root.</param>
    /// <param name="chavePublicaLicenciamento">
    /// Chave pública real da Weber Tech para verificar assinaturas de
    /// licença. Se omitida, é gerada uma chave descartável só para
    /// desenvolvimento — nenhuma licença emitida anteriormente vai
    /// validar depois de reiniciar a aplicação nesse cenário. O Desktop
    /// (Fase 6) deve fornecer a chave real assim que disponível (ver
    /// EnvelopeLicenca.cs para o aviso sobre o formato de licença ainda
    /// ser uma implementação de referência).
    /// </param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, RSA? chavePublicaLicenciamento = null)
    {
        services.AddSingleton<IArmazenamentoConfiguracaoLocal>(
            _ => new ArmazenamentoConfiguracaoLocalJson(CaminhosAplicacao.CaminhoConfiguracaoLocal));

        services.AddSingleton<ServicoImpressaoTexto>(
            _ => new ServicoImpressaoTexto(CaminhosAplicacao.PastaRecibos));

        services.AddSingleton<TransporteImpressoraUsb>();
        services.AddSingleton<IServicoImpressao, ServicoImpressaoEscPosUsb>();

        services.AddSingleton<IServicoBackup>(
            _ => new ServicoBackupSqlite(CaminhosAplicacao.CaminhoBaseDeDados));

        services.AddSingleton<ISenhaHasher, SenhaHasherPbkdf2>();

        var chavePublica = chavePublicaLicenciamento ?? FerramentasLicencaDeTeste.CriarParDeChaves();
        services.AddSingleton<IServicoLicenciamento>(
            _ => new ServicoLicenciamentoRsa(CaminhosAplicacao.CaminhoLicenca, chavePublica));

        // Fase 8, Parte 1: singleton (não scoped) porque o buffer de
        // timing entre teclas tem de sobreviver entre chamadas
        // sucessivas de ProcessarCaracter/ProcessarEnter ao longo de
        // toda a sessão do PDV — um scope por operação, como os casos
        // de uso da Application, destruiria o estado a meio de uma
        // leitura. Depende de IArmazenamentoConfiguracaoLocal, já
        // registado acima como singleton.
        services.AddSingleton<IServicoScanner>(
            provider => new ServicoScannerTeclado(provider.GetRequiredService<IArmazenamentoConfiguracaoLocal>()));

        return services;
    }
}
