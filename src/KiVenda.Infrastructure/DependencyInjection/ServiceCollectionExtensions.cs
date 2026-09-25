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
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        RSA? chavePublicaLicenciamento = null)
    {
        services.AddSingleton<IArmazenamentoConfiguracaoLocal>(
            _ => new ArmazenamentoConfiguracaoLocalJson(
                CaminhosAplicacao.CaminhoConfiguracaoLocal));

        services.AddSingleton<ServicoImpressaoTexto>(
            _ => new ServicoImpressaoTexto(CaminhosAplicacao.PastaRecibos));

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IDetectorImpressoras, DetectorImpressorasWindows>();
            services.AddSingleton<ITransporteImpressora, TransporteImpressoraWindows>();
        }
        else if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IDetectorImpressoras, DetectorImpressorasLinux>();
            services.AddSingleton<ITransporteImpressora, TransporteImpressoraLinux>();
        }
        else
        {
            services.AddSingleton<IDetectorImpressoras, DetectorImpressorasNaoSuportado>();
            services.AddSingleton<ITransporteImpressora, TransporteImpressoraNaoSuportado>();
        }

        services.AddSingleton<IServicoImpressao, ServicoImpressaoEscPosUsb>();

        services.AddSingleton<IServicoBackup>(
            _ => new ServicoBackupSqlite(CaminhosAplicacao.CaminhoBaseDeDados));

        services.AddSingleton<ISenhaHasher, SenhaHasherPbkdf2>();

        var chavePublica = chavePublicaLicenciamento ?? FerramentasLicencaDeTeste.CriarParDeChaves();
        services.AddSingleton<IServicoLicenciamento>(
            _ => new ServicoLicenciamentoRsa(CaminhosAplicacao.CaminhoLicenca, chavePublica));

        services.AddSingleton<IServicoScanner>(
            provider => new ServicoScannerTeclado(
                provider.GetRequiredService<IArmazenamentoConfiguracaoLocal>()));

        return services;
    }
}
