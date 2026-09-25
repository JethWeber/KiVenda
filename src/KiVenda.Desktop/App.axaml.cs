using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.DependencyInjection;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.Tema;
using KiVenda.Desktop.ViewModels;
using KiVenda.Desktop.ViewModels.Modulos;
using KiVenda.Desktop.Views;
using KiVenda.Infrastructure.Backup;
using KiVenda.Infrastructure.Caminhos;
using KiVenda.Infrastructure.DependencyInjection;
using KiVenda.Persistence;
using KiVenda.Persistence.DependencyInjection;
using KiVenda.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WeberTech.Licensing.Enums;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop;

public partial class App : Avalonia.Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();
        Services.GetRequiredService<ServicoTema>().CarregarEAplicar();
        InicializarBaseDeDadosAsync().GetAwaiter().GetResult();
        InicializarLicenciamento();
        Services.GetRequiredService<KiVenda.Desktop.Notificacoes.ServicoMonitorLicenca>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InicializarLicenciamento()
    {
        if (!OperatingSystem.IsWindows())
        {
            Log.Information("Licenciamento Weber Tech: inicialização real adiada porque o ambiente não é Windows.");
            return;
        }

        try
        {
            Licensing.Initialize(ProductType.KiVenda, "kivenda.desktop_v03");
            Log.Information("Licenciamento Weber Tech inicializado. Estado: {Estado}", Licensing.CurrentStatus);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Falha ao inicializar o licenciamento Weber Tech.");
        }
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddPersistence(CaminhosAplicacao.CaminhoBaseDeDados);
        services.AddApplicationUseCases();
        services.AddInfrastructure();
        services.AddSingleton<IServicoBackup>(_ => new ServicoBackupSqlite(CaminhosAplicacao.CaminhoBaseDeDados));
        services.AddSingleton<ServicoTema>();
        services.AddSingleton<KiVenda.Desktop.Notificacoes.ServicoNotificacoes>();
        services.AddSingleton<KiVenda.Desktop.Notificacoes.ServicoMonitorLicenca>();
        services.AddTransient<ConfiguracoesViewModel>();

        services.AddSingleton<SessaoUtilizadorAtual>();
        services.AddSingleton<IContextoAutenticacao>(sp => sp.GetRequiredService<SessaoUtilizadorAtual>());
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }

    private static async Task InicializarBaseDeDadosAsync()
    {
        Log.Information("A preparar a base de dados local...");
        await using var scope = Services.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<KiVendaDbContext>();
        await contexto.Database.MigrateAsync();
        var senhaHasher = scope.ServiceProvider.GetRequiredService<ISenhaHasher>();
        await KiVendaDbSeeder.SeedAsync(contexto, senhaHasher.GerarHash("admin123"));
        Log.Information("Base de dados pronta.");
    }
}
