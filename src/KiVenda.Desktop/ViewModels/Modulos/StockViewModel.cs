using System.ComponentModel;
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

    public bool MostrarProdutos => AbaSelecionada == 0;
    public bool MostrarAbastecer => AbaSelecionada == 1;

    // ── Aviso (toast) de sucesso/erro: aparece 3 s e some sozinho ──
    private CancellationTokenSource? _toastCts;

    [ObservableProperty]
    private bool _toastVisivel;

    [ObservableProperty]
    private bool _toastErro;

    [ObservableProperty]
    private string _toastTitulo = string.Empty;

    [ObservableProperty]
    private string _toastMensagem = string.Empty;

    partial void OnAbaSelecionadaChanged(int value)
    {
        OnPropertyChanged(nameof(MostrarProdutos));
        OnPropertyChanged(nameof(MostrarAbastecer));
    }

    public StockViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        Produtos = new ProdutosViewModel(scopeFactory, sessao);
        Abastecer = new ComprasViewModel(scopeFactory);

        // Sucesso: os ViewModels filhos avisam quando guardam com sucesso.
        Produtos.GuardadoComSucesso += (_, mensagem) => MostrarToast(erro: false, mensagem);
        Abastecer.GuardadoComSucesso += (_, mensagem) => MostrarToast(erro: false, mensagem);

        // Erro: qualquer mensagem de erro (da lista ou do formulário) vira aviso.
        Produtos.PropertyChanged += OnFilhoPropertyChanged;
        Abastecer.PropertyChanged += OnFilhoPropertyChanged;
    }

    private void OnFilhoPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        string? mensagem = (sender, e.PropertyName) switch
        {
            (ProdutosViewModel p, nameof(ProdutosViewModel.MensagemErro)) => p.MensagemErro,
            (ProdutosViewModel p, nameof(ProdutosViewModel.MensagemErroFormulario)) => p.MensagemErroFormulario,
            (ComprasViewModel c, nameof(ComprasViewModel.MensagemErro)) => c.MensagemErro,
            (ComprasViewModel c, nameof(ComprasViewModel.MensagemErroFormulario)) => c.MensagemErroFormulario,
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(mensagem))
            MostrarToast(erro: true, mensagem);
    }

    private async void MostrarToast(bool erro, string mensagem)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();

        ToastErro = erro;
        ToastTitulo = erro ? "Erro" : "Sucesso";
        ToastMensagem = mensagem;
        ToastVisivel = true;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            ToastVisivel = false;
        }
        catch (OperationCanceledException)
        {
            // Chegou outro aviso: ele reinicia a contagem.
        }
    }

    [RelayCommand]
    private void SelecionarProdutos() => AbaSelecionada = 0;

    [RelayCommand]
    private void SelecionarAbastecer() => AbaSelecionada = 1;
}
