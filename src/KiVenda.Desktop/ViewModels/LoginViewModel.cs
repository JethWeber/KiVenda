using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.Autenticacao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// Login local (Secção 3 da documentação funcional: sem servidor, sem
/// internet). Cada tentativa de login cria o seu próprio
/// <see cref="IServiceScope"/> para resolver <see cref="AutenticarUtilizadorUseCase"/>
/// — isto garante um <c>IUnitOfWork</c>/DbContext novo por tentativa,
/// em vez de uma única instância a viver durante toda a aplicação (o
/// padrão a seguir também nas fases seguintes sempre que a UI invoca um
/// caso de uso).
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;
    private readonly ServicoBloqueioLogin _bloqueioLogin;
    private readonly DispatcherTimer _timerBloqueio;

    [ObservableProperty]
    private string _nomeUtilizador = string.Empty;

    [ObservableProperty]
    private string _senha = string.Empty;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private bool _aEntrar;

    [ObservableProperty]
    private bool _mostrarSenha;

    [ObservableProperty]
    private bool _estaBloqueado;

    [ObservableProperty]
    private string _tempoBloqueio = string.Empty;

    public char PasswordChar => MostrarSenha ? '\0' : '•';

    public event EventHandler<UtilizadorAutenticadoDto>? LoginBemSucedido;

    public LoginViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao, ServicoBloqueioLogin bloqueioLogin)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        _bloqueioLogin = bloqueioLogin;

        _timerBloqueio = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timerBloqueio.Tick += (_, _) => AtualizarBloqueio();
        _timerBloqueio.Start();
    }

    private void AtualizarBloqueio()
    {
        if (string.IsNullOrWhiteSpace(NomeUtilizador))
        {
            EstaBloqueado = false;
            TempoBloqueio = string.Empty;
            return;
        }

        if (_bloqueioLogin.PodeTentar(NomeUtilizador, out var restante))
        {
            EstaBloqueado = false;
            TempoBloqueio = string.Empty;
            return;
        }

        EstaBloqueado = true;
        TempoBloqueio = FormatarTempo(restante);
    }

    partial void OnMostrarSenhaChanged(bool value)
    {
        OnPropertyChanged(nameof(PasswordChar));
    }

    [RelayCommand]
    private void AlternarVisibilidadeSenha()
    {
        MostrarSenha = !MostrarSenha;
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        MensagemErro = null;

        AtualizarBloqueio();
        if (EstaBloqueado)
        {
            MensagemErro = $"Demasiadas tentativas. Tente novamente em {TempoBloqueio}.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NomeUtilizador) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Indique o nome de utilizador e a password.";
            return;
        }

        AEntrar = true;
        try
        {
            // CreateAsyncScope (não CreateScope): o UnitOfWork da
            // Persistence só implementa IAsyncDisposable (o DbContext do
            // EF Core é fechado via DisposeAsync), por isso o scope tem de
            // ser descartado de forma assíncrona também — um "using"
            // síncrono aqui lança InvalidOperationException ao tentar
            // fechar o container.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var autenticarUseCase = scope.ServiceProvider.GetRequiredService<AutenticarUtilizadorUseCase>();

            var utilizador = await autenticarUseCase.ExecutarAsync(new AutenticarUtilizadorCommand(NomeUtilizador, Senha));

            _bloqueioLogin.RegistarSucesso(NomeUtilizador);
            _sessao.IniciarSessao(utilizador.UtilizadorId, utilizador.Nome, utilizador.Perfil);
            LoginBemSucedido?.Invoke(this, utilizador);
        }
        catch (DomainException ex)
        {
            TimeSpan bloqueio = _bloqueioLogin.RegistarFalha(NomeUtilizador);
            MensagemErro = bloqueio > TimeSpan.Zero
                ? $"Demasiadas tentativas. Acesso bloqueado por {FormatarDuracao(bloqueio)}."
                : ex.Message;

            AtualizarBloqueio();
        }
        finally
        {
            AEntrar = false;
        }
    }

    /// <summary>Limpa o formulário — chamado ao regressar ao login depois de um "Terminar sessão".</summary>
    public void Reiniciar()
    {
        NomeUtilizador = string.Empty;
        Senha = string.Empty;
        MensagemErro = null;
        AEntrar = false;
        MostrarSenha = false;
        AtualizarBloqueio();
    }

    private static string FormatarTempo(TimeSpan restante)
    {
        if (restante.TotalHours >= 1)
            return $"{(int)restante.TotalHours}h {restante.Minutes:00}min";

        if (restante.TotalMinutes >= 1)
            return $"{(int)restante.TotalMinutes}min {restante.Seconds:00}s";

        return $"{Math.Max(1, restante.Seconds)}s";
    }

    private static string FormatarDuracao(TimeSpan duracao)
    {
        if (duracao.TotalHours >= 1)
            return $"{(int)duracao.TotalHours} hora(s)";

        return $"{(int)duracao.TotalMinutes} minuto(s)";
    }
}
