using System.Reflection;
using System.Text.Json;
using KiVenda.Infrastructure.Caminhos;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop.Notificacoes;

public sealed class ServicoMonitorLicenca : IDisposable
{
    private readonly ServicoNotificacoes _notificacoes;
    private readonly Timer _timer;
    private readonly string _estadoPath;
    private DateTime? _ultimaNotificacao;

    public ServicoMonitorLicenca(ServicoNotificacoes notificacoes)
    {
        _notificacoes = notificacoes;
        _estadoPath = Path.Combine(CaminhosAplicacao.PastaDados, "notificacao-licenca.json");
        _ultimaNotificacao = CarregarUltimaNotificacao();
        _timer = new Timer(_ => Verificar(), null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
    }

    private void Verificar()
    {
        try
        {
            var expiracao = ObterDataExpiracao();
            if (expiracao is null)
                return;

            var dias = (expiracao.Value.Date - DateTime.Today).Days;
            if (dias < 0 || dias > 90)
                return;

            if (_ultimaNotificacao.HasValue && DateTime.Now - _ultimaNotificacao.Value < TimeSpan.FromDays(3))
                return;

            _notificacoes.Adicionar(
                "Licença a expirar",
                $"A licença do KiVenda expira em {dias} dia(s), em {expiracao.Value:dd/MM/yyyy}.");

            _ultimaNotificacao = DateTime.Now;
            GuardarUltimaNotificacao();
        }
        catch
        {
            // O monitor nunca deve impedir o arranque ou funcionamento do KiVenda.
        }
    }

    private static DateTime? ObterDataExpiracao()
    {
        var tipo = typeof(Licensing);
        var nomes = new[] { "ExpirationDate", "ExpiryDate", "ExpiresAt", "Expiration", "Expiry" };

        foreach (var nome in nomes)
        {
            var propriedade = tipo.GetProperty(nome, BindingFlags.Public | BindingFlags.Static);
            if (propriedade?.GetValue(null) is DateTime data)
                return data;

            if (propriedade?.GetValue(null) is DateTimeOffset offset)
                return offset.LocalDateTime;
        }

        return null;
    }

    private DateTime? CarregarUltimaNotificacao()
    {
        try
        {
            if (!File.Exists(_estadoPath))
                return null;

            var json = File.ReadAllText(_estadoPath);
            return JsonSerializer.Deserialize<DateTime?>(json);
        }
        catch
        {
            return null;
        }
    }

    private void GuardarUltimaNotificacao()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_estadoPath)!);
            File.WriteAllText(_estadoPath, JsonSerializer.Serialize(_ultimaNotificacao));
        }
        catch
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
