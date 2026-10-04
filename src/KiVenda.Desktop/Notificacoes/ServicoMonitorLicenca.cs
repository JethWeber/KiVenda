using System.Text.Json;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Infrastructure.Caminhos;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.Notificacoes;

public sealed class ServicoMonitorLicenca : IDisposable
{
    private const string Tipo = "LICENCA_EXPIRANDO";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;
    private readonly ServicoNotificacoes _notificacoes;
    private readonly Timer _timer;
    private readonly string _estadoPath;
    private DateTime? _ultimaNotificacao;

    public ServicoMonitorLicenca(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao, ServicoNotificacoes notificacoes)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        _notificacoes = notificacoes;
        _estadoPath = Path.Combine(CaminhosAplicacao.PastaDados, "notificacao-licenca.json");
        _ultimaNotificacao = CarregarUltimaNotificacao();
        _timer = new Timer(_ => _ = VerificarAsync(), null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
    }

    private async Task VerificarAsync()
    {
        try
        {
            var politica = PoliticaLicencaKiVenda.Avaliar();

            if (_sessao.UtilizadorId == Guid.Empty ||
                politica.Acesso is not AcessoLicenca.Aviso and not AcessoLicenca.Tolerancia)
                return;

            if (_ultimaNotificacao.HasValue &&
                DateTime.UtcNow - _ultimaNotificacao.Value < TimeSpan.FromDays(3))
                return;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var ultima = await uow.Notificacoes.ObterUltimaPorTipoAsync(_sessao.UtilizadorId, Tipo);

            if (ultima is not null && DateTime.UtcNow - ultima.DataCriacao < TimeSpan.FromDays(3))
                return;

            await _notificacoes.AdicionarAsync(Tipo, politica.Titulo, politica.Mensagem);
            _ultimaNotificacao = DateTime.UtcNow;
            GuardarUltimaNotificacao();
        }
        catch
        {
            // O monitor nunca impede o funcionamento do KiVenda.
        }
    }

    private DateTime? CarregarUltimaNotificacao()
    {
        try
        {
            if (!File.Exists(_estadoPath))
                return null;

            return JsonSerializer.Deserialize<DateTime?>(File.ReadAllText(_estadoPath));
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
            string? pasta = Path.GetDirectoryName(_estadoPath);
            if (!string.IsNullOrWhiteSpace(pasta))
                Directory.CreateDirectory(pasta);

            File.WriteAllText(_estadoPath, JsonSerializer.Serialize(_ultimaNotificacao));
        }
        catch
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
