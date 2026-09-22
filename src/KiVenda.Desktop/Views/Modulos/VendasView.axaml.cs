using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KiVenda.Application.Produtos;
using KiVenda.Application.Vendas;
using KiVenda.Desktop.ViewModels.Modulos;
using KiVenda.Infrastructure.Scanner;

namespace KiVenda.Desktop.Views.Modulos;

public partial class VendasView : UserControl
{
    private IServicoScanner? _servicoScanner;

    public VendasView()
    {
        InitializeComponent();
    }

    private void VendasView_AttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _servicoScanner = App.Services.GetService(typeof(IServicoScanner)) as IServicoScanner;
        if (_servicoScanner is null)
        {
            return;
        }

        _servicoScanner.CodigoLido -= OnCodigoLido;
        _servicoScanner.CodigoLido += OnCodigoLido;

        _ = RecarregarScannerAsync();
        PesquisaCodigoTextBox.Focus();
    }

    private void VendasView_DetachedFromVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (_servicoScanner is not null)
        {
            _servicoScanner.CodigoLido -= OnCodigoLido;
            _servicoScanner.Reset();
        }
    }

    private void OnCodigoLido(string codigo)
    {
        if (DataContext is VendasViewModel vm)
        {
            _ = vm.ProcessarLeituraScannerAsync(codigo);
        }
    }

    private async Task RecarregarScannerAsync()
    {
        try
        {
            if (_servicoScanner is not null)
            {
                await _servicoScanner.RecarregarConfiguracaoAsync();
            }
        }
        catch
        {
            // Configuração inexistente/corrompida não impede o PDV de abrir;
            // o serviço usa a configuração padrão.
        }
    }

    private void PesquisaCodigoTextBox_TextInput(object? sender, TextInputEventArgs e)
    {
        if (_servicoScanner is null || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        foreach (var caractere in e.Text)
        {
            _servicoScanner.ProcessarCaracter(caractere);
        }
    }

    private void PesquisaCodigoTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        _servicoScanner?.ProcessarEnter();

        // Se o Enter não fechou uma leitura de scanner, o ViewModel
        // preserva o fluxo manual: código exato -> produto -> adicionar.
        if (DataContext is VendasViewModel vm)
        {
            _ = vm.TratarEnterPesquisaAsync();
        }

        e.Handled = true;
    }

    /// <summary>
    /// Usa Click + Tag (em vez de Command com binding relativo ao
    /// DataContext do módulo) porque o botão vive dentro do
    /// DataTemplate de <see cref="ProdutoDto"/> — mais simples e menos
    /// arriscado do que sintaxe de binding "$parent" em bindings
    /// compilados.
    /// </summary>
    private void BotaoAdicionarProduto_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ProdutoDto produto } && DataContext is VendasViewModel vm)
        {
            vm.AdicionarProdutoCommand.Execute(produto);
        }
    }

    private void BotaoRemoverItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ItemVendaDto item } && DataContext is VendasViewModel vm)
        {
            vm.RemoverItemCommand.Execute(item);
        }
    }
}
