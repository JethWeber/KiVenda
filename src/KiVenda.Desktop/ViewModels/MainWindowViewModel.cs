using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Application.Utilizadores;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// Anfitrião do conteúdo da janela principal: alterna entre
/// <see cref="LoginViewModel"/> e <see cref="ShellViewModel"/> (a shell
/// definitiva construída na Fase 6, substituindo o placeholder
/// "BemVindoViewModel" da Fase 5).
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly LoginViewModel _loginViewModel;
    private readonly SessaoUtilizadorAtual _sessao;
    private readonly IServiceScopeFactory _scopeFactory;

    [ObservableProperty]
    private ViewModelBase _conteudoAtual;

    public MainWindowViewModel(LoginViewModel loginViewModel, SessaoUtilizadorAtual sessao, IServiceScopeFactory scopeFactory)
    {
        _loginViewModel = loginViewModel;
        _sessao = sessao;
        _scopeFactory = scopeFactory;

        _loginViewModel.LoginBemSucedido += OnLoginBemSucedido;
        _conteudoAtual = _loginViewModel;
    }

    private void OnLoginBemSucedido(object? sender, UtilizadorAutenticadoDto utilizador)
    {
        var shell = new ShellViewModel(_scopeFactory, _sessao);
        shell.SessaoTerminada += OnSessaoTerminada;

        ConteudoAtual = shell;
    }

    private void OnSessaoTerminada(object? sender, EventArgs e)
    {
        if (sender is ShellViewModel shell)
        {
            shell.SessaoTerminada -= OnSessaoTerminada;
        }

        _loginViewModel.Reiniciar();
        ConteudoAtual = _loginViewModel;
    }
}
