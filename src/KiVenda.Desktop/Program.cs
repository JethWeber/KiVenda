using System;
using Avalonia;
using KiVenda.Infrastructure.Caminhos;
using Serilog;

namespace KiVenda.Desktop;

internal static class Program
{
    // Ponto de entrada convencional do Avalonia.
    // Não deve conter lógica de inicialização do AppBuilder além do
    // estritamente necessário — isso vive em App.axaml.cs.
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureLogging();

        try
        {
            Log.Information("A iniciar o KiVenda Desktop...");

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Falha fatal ao iniciar o KiVenda Desktop");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureLogging()
    {
        // Pasta de dados da aplicação (Fase 4: KiVenda.Infrastructure),
        // em vez de um caminho relativo — "logs/" dependia de onde o
        // processo era arrancado (ex.: `dotnet run` vs. duplo-clique no
        // executável instalado), o que não é fiável para uma app Desktop.
        var caminhoLog = System.IO.Path.Combine(CaminhosAplicacao.PastaLogs, "kivenda-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                path: caminhoLog,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();
    }
}
