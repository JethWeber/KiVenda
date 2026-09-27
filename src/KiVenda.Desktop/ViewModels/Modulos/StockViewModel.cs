using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class StockViewModel : ViewModelBase
{
    public ProdutosViewModel Produtos { get; }
    public ComprasViewModel Abastecer { get; }

    [ObservableProperty]
    private int _abaSelecionada;

    public StockViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        Produtos = new ProdutosViewModel(scopeFactory, sessao);
        Abastecer = new ComprasViewModel(scopeFactory);
    }

    [RelayCommand]
    private void SelecionarProdutos() => AbaSelecionada = 0;

    [RelayCommand]
    private void SelecionarAbastecer() => AbaSelecionada = 1;
}
