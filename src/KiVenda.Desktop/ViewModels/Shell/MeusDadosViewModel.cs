using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.Autenticacao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Shell;

public partial class MeusDadosViewModel : ViewModelBase
{
    private readonly SessaoUtilizadorAtual _sessao;

    [ObservableProperty] private string _nome;
    [ObservableProperty] private string _nomeUtilizador;
    [ObservableProperty] private string _novaSenha = string.Empty;
    [ObservableProperty] private string? _mensagem;

    public event EventHandler? GuardadoComSucesso;
    public event EventHandler? Cancelado;

    public MeusDadosViewModel(SessaoUtilizadorAtual sessao)
    {
        _sessao = sessao;
        _nome = sessao.Nome;
        _nomeUtilizador = string.Empty;
        _ = CarregarLoginAsync();
    }

    private async Task CarregarLoginAsync()
    {
        try
        {
            await using var scope = App.Services.CreateAsyncScope();
            var utilizador = await scope.ServiceProvider.GetRequiredService<KiVenda.Application.Abstractions.Persistence.IUnitOfWork>()
                .Utilizadores.ObterPorIdAsync(_sessao.UtilizadorId);
            NomeUtilizador = utilizador?.NomeUtilizador ?? string.Empty;
        }
        catch { }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task GuardarAsync()
    {
        try
        {
            await using var scope = App.Services.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<EditarMeusDadosUseCase>();
            await useCase.ExecutarAsync(new EditarMeusDadosCommand(_sessao.UtilizadorId, Nome, NomeUtilizador, NovaSenha));

            Mensagem = "Dados atualizados. Será necessário iniciar sessão novamente.";
            GuardadoComSucesso?.Invoke(this, EventArgs.Empty);
        }
        catch (DomainException ex)
        {
            Mensagem = ex.Message;
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao guardar: {ex.Message}";
        }
    }
}
