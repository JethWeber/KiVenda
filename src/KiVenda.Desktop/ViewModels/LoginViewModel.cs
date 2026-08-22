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

    [ObservableProperty]
    private string _nomeUtilizador = string.Empty;

    [ObservableProperty]
    private string _senha = string.Empty;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private bool _aEntrar;

    public event EventHandler<UtilizadorAutenticadoDto>? LoginBemSucedido;

    public LoginViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        MensagemErro = null;

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

            _sessao.IniciarSessao(utilizador.UtilizadorId, utilizador.Nome, utilizador.Perfil);
            LoginBemSucedido?.Invoke(this, utilizador);
        }
        catch (DomainException ex)
        {
            // AutenticarUtilizadorUseCase devolve sempre a mesma mensagem
            // genérica ("Utilizador ou password inválidos.") — ver Fase 3.
            MensagemErro = ex.Message;
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
    }
}
