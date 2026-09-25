using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Styling;
using KiVenda.Infrastructure.Caminhos;

namespace KiVenda.Desktop.Tema;

public sealed class ServicoTema
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public TemaKiVenda TemaAtual { get; private set; } = TemaKiVenda.Sistema;

    public void CarregarEAplicar()
    {
        try
        {
            if (File.Exists(CaminhosAplicacao.CaminhoTema))
            {
                var json = File.ReadAllText(CaminhosAplicacao.CaminhoTema);
                var configuracao = JsonSerializer.Deserialize<ConfiguracaoTema>(json, _jsonOptions);
                if (configuracao is not null)
                    TemaAtual = configuracao.Tema;
            }
        }
        catch
        {
            TemaAtual = TemaKiVenda.Sistema;
        }

        Aplicar(TemaAtual);
    }

    public void DefinirTema(TemaKiVenda tema)
    {
        TemaAtual = tema;
        Aplicar(tema);

        var configuracao = new ConfiguracaoTema(tema);
        var json = JsonSerializer.Serialize(configuracao, _jsonOptions);
        File.WriteAllText(CaminhosAplicacao.CaminhoTema, json);
    }

    private static void Aplicar(TemaKiVenda tema)
    {
        if (Application.Current is not { } app)
            return;

        app.RequestedThemeVariant = tema switch
        {
            TemaKiVenda.Dark => ThemeVariant.Dark,
            TemaKiVenda.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }

    private sealed record ConfiguracaoTema(TemaKiVenda Tema);
}