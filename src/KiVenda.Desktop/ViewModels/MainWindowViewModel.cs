using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Application.Utilizadores;
using KiVenda.Desktop.Autenticacao;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// Anfitrião do conteúdo da janela principal. Nesta fase, alterna entre
/// <see cref="LoginViewModel"/> e <see cref="BemVindoViewModel"/> — a
/// partir da Fase 6, <see cref="ConteudoAtual"/> passa a alternar entre
/// a shell definitiva (menu lateral + módulos) e o login, mas o padrão
/// de navegação por troca de ViewModel mantém-se o mesmo.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LoginViewModel _loginViewModel;
    private readonly SessaoUtilizadorAtual _sessao;

    [ObservableProperty]
    private ViewModelBase _conteudoAtual;

    public MainWindowViewModel(LoginViewModel loginViewModel, SessaoUtilizadorAtual sessao)
    {
        _loginViewModel = loginViewModel;
        _sessao = sessao;

        _loginViewModel.LoginBemSucedido += OnLoginBemSucedido;
        _conteudoAtual = _loginViewModel;
    }

    private void OnLoginBemSucedido(object? sender, UtilizadorAutenticadoDto utilizador)
    {
        var bemVindo = new BemVindoViewModel(utilizador, _sessao);
        bemVindo.SessaoTerminada += OnSessaoTerminada;

        ConteudoAtual = bemVindo;
    }

    private void OnSessaoTerminada(object? sender, EventArgs e)
    {
        if (sender is BemVindoViewModel bemVindo)
        {
            bemVindo.SessaoTerminada -= OnSessaoTerminada;
        }

        _loginViewModel.Reiniciar();
        ConteudoAtual = _loginViewModel;
    }
}
