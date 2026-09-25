using System.Reflection;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Desktop.Autenticacao;
using Microsoft.Extensions.DependencyInjection;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop.Notificacoes;

public sealed class ServicoMonitorLicenca : IDisposable
{
    private const string Tipo = "LICENCA_EXPIRANDO";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;
    private readonly ServicoNotificacoes _notificacoes;
    private readonly Timer _timer;

    public ServicoMonitorLicenca(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao, ServicoNotificacoes notificacoes)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        _notificacoes = notificacoes;
        _timer = new Timer(_ => _ = VerificarAsync(), null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
    }

    private async Task VerificarAsync()
    {
        try
        {
            if (_sessao.UtilizadorId == Guid.Empty) return;
            var expiracao = ObterDataExpiracao();
            if (expiracao is null) return;
            var dias = (expiracao.Value.Date - DateTime.Today).Days;
            if (dias < 0 || dias > 90) return;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var ultima = await uow.Notificacoes.ObterUltimaPorTipoAsync(_sessao.UtilizadorId, Tipo);
            if (ultima is not null && DateTime.UtcNow - ultima.DataCriacao < TimeSpan.FromDays(3)) return;

            await _notificacoes.AdicionarAsync(Tipo, "Licença a expirar",
                $"A licença do KiVenda expira em {dias} dia(s), em {expiracao.Value:dd/MM/yyyy}.");
        }
        catch { }
    }

    private static DateTime? ObterDataExpiracao()
    {
        var tipo = typeof(Licensing);
        foreach (var nome in new[] { "ExpirationDate", "ExpiryDate", "ExpiresAt", "Expiration", "Expiry" })
        {
            var propriedade = tipo.GetProperty(nome, BindingFlags.Public | BindingFlags.Static);
            var valor = propriedade?.GetValue(null);
            if (valor is DateTime data) return data;
            if (valor is DateTimeOffset offset) return offset.LocalDateTime;
        }
        return null;
    }

    public void Dispose() => _timer.Dispose();
}