using System.Text.Json;
using KiVenda.Infrastructure.Caminhos;

namespace KiVenda.Desktop.Autenticacao;

/// <summary>
/// Controla tentativas de autenticação por nome de utilizador, com estado
/// persistente para sobreviver a reinícios da aplicação.
/// Escalonamento: 5 falhas → 5 min; +3 → 30 min; +3 → 3 h; +3 → 24 h.
/// Uma autenticação bem-sucedida limpa o histórico desse utilizador.
/// </summary>
public sealed class ServicoBloqueioLogin
{
    private const int PrimeiroNivel = 5;
    private static readonly TimeSpan[] Duracoes =
    [
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(3),
        TimeSpan.FromHours(24)
    ];

    private readonly object _sync = new();
    private readonly string _caminho;
    private Dictionary<string, EstadoBloqueio> _estados;

    public ServicoBloqueioLogin()
    {
        _caminho = Path.Combine(CaminhosAplicacao.PastaDados, "bloqueios-login.json");
        _estados = Carregar();
    }

    public bool PodeTentar(string nomeUtilizador, out TimeSpan restante)
    {
        string chave = Normalizar(nomeUtilizador);

        lock (_sync)
        {
            if (!_estados.TryGetValue(chave, out var estado) || estado.BloqueadoAteUtc is null)
            {
                restante = TimeSpan.Zero;
                return true;
            }

            restante = estado.BloqueadoAteUtc.Value - DateTime.UtcNow;
            if (restante <= TimeSpan.Zero)
            {
                estado = estado with { BloqueadoAteUtc = null };
                _estados[chave] = estado;
                Guardar();
                restante = TimeSpan.Zero;
                return true;
            }

            return false;
        }
    }

    public TimeSpan RegistarFalha(string nomeUtilizador)
    {
        string chave = Normalizar(nomeUtilizador);

        lock (_sync)
        {
            _estados.TryGetValue(chave, out var estado);
            int falhas = (estado?.Falhas ?? 0) + 1;

            TimeSpan? duracao = ObterDuracao(falhas);
            DateTime? bloqueadoAte = duracao.HasValue
                ? DateTime.UtcNow.Add(duracao.Value)
                : null;

            _estados[chave] = new EstadoBloqueio(falhas, bloqueadoAte);
            Guardar();

            return duracao ?? TimeSpan.Zero;
        }
    }

    public void RegistarSucesso(string nomeUtilizador)
    {
        string chave = Normalizar(nomeUtilizador);

        lock (_sync)
        {
            if (_estados.Remove(chave))
                Guardar();
        }
    }

    public int ObterFalhas(string nomeUtilizador)
    {
        lock (_sync)
            return _estados.TryGetValue(Normalizar(nomeUtilizador), out var estado)
                ? estado.Falhas
                : 0;
    }

    private static TimeSpan? ObterDuracao(int falhas)
    {
        if (falhas < PrimeiroNivel)
            return null;

        int nivel = Math.Min((falhas - PrimeiroNivel) / 3, Duracoes.Length - 1);
        return Duracoes[nivel];
    }

    private Dictionary<string, EstadoBloqueio> Carregar()
    {
        try
        {
            if (!File.Exists(_caminho))
                return new(StringComparer.OrdinalIgnoreCase);

            var json = File.ReadAllText(_caminho);
            return JsonSerializer.Deserialize<Dictionary<string, EstadoBloqueio>>(json)
                ?? new(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void Guardar()
    {
        try
        {
            string? pasta = Path.GetDirectoryName(_caminho);
            if (!string.IsNullOrWhiteSpace(pasta))
                Directory.CreateDirectory(pasta);

            string temporario = _caminho + ".tmp";
            File.WriteAllText(temporario, JsonSerializer.Serialize(_estados));
            File.Move(temporario, _caminho, overwrite: true);
        }
        catch
        {
            // O bloqueio em memória continua ativo mesmo que a persistência falhe.
        }
    }

    private static string Normalizar(string nomeUtilizador) =>
        (nomeUtilizador ?? string.Empty).Trim().ToUpperInvariant();

    private sealed record EstadoBloqueio(int Falhas, DateTime? BloqueadoAteUtc);
}
