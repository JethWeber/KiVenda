using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.DependencyInjection;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels;
using KiVenda.Desktop.Views;
using KiVenda.Infrastructure.Caminhos;
using KiVenda.Infrastructure.DependencyInjection;
using KiVenda.Persistence;
using KiVenda.Persistence.DependencyInjection;
using KiVenda.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace KiVenda.Desktop;

public partial class App : Avalonia.Application
{
    /// <summary>
    /// Composition root da aplicação. Exposto como estático por simplicidade
    /// nesta fase inicial; se necessário, poderá ser substituído por um
    /// IHost completo (Microsoft.Extensions.Hosting) nas fases seguintes.
    /// </summary>
    public static IServiceProvider Services { get; private set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ConfigureServices();

        // Ponto de arranque único: garante que a base de dados existe e
        // está semeada (unidades de medida, categoria "Geral", utilizador
        // Gerente inicial) antes de qualquer ecrã aparecer — suporta o
        // princípio "instalar e vender em 5 minutos" (Secção 3).
        //
        // Chamada bloqueante deliberada: neste ponto exato o loop de
        // mensagens do Avalonia ainda não arrancou (estamos antes de
        // StartWithClassicDesktopLifetime terminar), por isso não há
        // risco de deadlock por sincronizar sobre código assíncrono aqui.
        InicializarBaseDeDadosAsync().GetAwaiter().GetResult();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddPersistence(CaminhosAplicacao.CaminhoBaseDeDados);
        services.AddApplicationUseCases();
        services.AddInfrastructure();

        // Sessão de utilizador: registada como singleton concreto (para
        // que LoginViewModel/BemVindoViewModel possam chamar
        // IniciarSessao/TerminarSessao, que não fazem parte do contrato
        // IContextoAutenticacao) e também exposta através do contrato,
        // resolvendo sempre a MESMA instância.
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

        // Migração real (dotnet ef migrations add InicialCreate) ainda
        // pendente — ver Fase 2 do plano de implementação. EnsureCreated
        // já é suficiente para a app arrancar e para o seed funcionar.
        await contexto.Database.EnsureCreatedAsync();

        var senhaHasher = scope.ServiceProvider.GetRequiredService<ISenhaHasher>();

        // Password inicial do Gerente semeado na primeira execução.
        // TODO (Fase 11): forçar troca desta password no primeiro login,
        // em vez de a deixar fixa indefinidamente.
        await KiVendaDbSeeder.SeedAsync(contexto, senhaHasher.GerarHash("admin123"));

        Log.Information("Base de dados pronta.");
    }
}
