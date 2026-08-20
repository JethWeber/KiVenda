using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KiVenda.Desktop.ViewModels;
using KiVenda.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop;

public partial class App : Application
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

        // Fase 0: apenas o necessário para a janela inicial abrir.
        //
        // A partir das fases seguintes, este método passará a delegar em
        // extension methods dedicados por camada, por exemplo:
        //
        //   services.AddCore();
        //   services.AddApplicationLayer();   // Fase 3
        //   services.AddInfrastructure();     // Fase 4
        //   services.AddPersistence();        // Fase 2
        //   services.AddDesktopViewModels();  // Fase 6+
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
