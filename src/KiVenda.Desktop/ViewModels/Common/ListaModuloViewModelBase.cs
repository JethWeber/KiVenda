using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Common;

/// <summary>
/// Base para módulos cujo ecrã principal é "carregar uma lista, com
/// pesquisa opcional, via um caso de uso". Cada carregamento cria e
/// descarta o seu próprio scope (ver <see cref="CarregarAsync"/>),
/// seguindo a mesma convenção estabelecida na Fase 5.
/// </summary>
public abstract partial class ListaModuloViewModelBase<TDto> : ViewModelBase
{
    protected readonly IServiceScopeFactory ScopeFactory;

    [ObservableProperty]
    private string _termoPesquisa = string.Empty;

    [ObservableProperty]
    private bool _aCarregar;

    [ObservableProperty]
    private string? _mensagemErro;

    public ObservableCollection<TDto> Itens { get; } = new();

    protected ListaModuloViewModelBase(IServiceScopeFactory scopeFactory)
    {
        ScopeFactory = scopeFactory;
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        ACarregar = true;
        MensagemErro = null;

        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();
            var itens = await ObterItensAsync(scope.ServiceProvider);

            Itens.Clear();
            foreach (var item in itens)
            {
                Itens.Add(item);
            }
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            ACarregar = false;
        }
    }

    partial void OnTermoPesquisaChanged(string value) => _ = CarregarAsync();

    protected abstract Task<IReadOnlyList<TDto>> ObterItensAsync(IServiceProvider servicos);
}
