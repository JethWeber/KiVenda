using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Empresas;
using KiVenda.Application.Produtos;
using KiVenda.Application.Vendas;
using KiVenda.Desktop.ViewModels.Modulos;
using KiVenda.Desktop.Views.Dialogos;
using KiVenda.Infrastructure.Impressao;
using KiVenda.Infrastructure.Scanner;
using Microsoft.Extensions.DependencyInjection;

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

        if (DataContext is VendasViewModel vm)
        {
            vm.SolicitarImpressaoRecibo -= OnSolicitarImpressaoReciboAsync;
            vm.SolicitarImpressaoRecibo += OnSolicitarImpressaoReciboAsync;
        }

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
        if (DataContext is VendasViewModel vm)
        {
            vm.SolicitarImpressaoRecibo -= OnSolicitarImpressaoReciboAsync;
        }

        if (_servicoScanner is not null)
        {
            _servicoScanner.CodigoLido -= OnCodigoLido;
            _servicoScanner.Reset();
        }
    }

    private async Task<bool> OnSolicitarImpressaoReciboAsync(ReciboVendaDto recibo)
    {
        if (TopLevel.GetTopLevel(this) is not Window owner)
        {
            return false;
        }

        var dialogo = new ConfirmarImpressaoWindow();
        var imprimir = await dialogo.ShowDialog<bool>(owner);

        if (!imprimir)
        {
            return false;
        }

        await using var scope = App.Services.CreateAsyncScope();
        var empresa = await scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>()
            .Empresas
            .ObterAsync();

        var dadosLoja = empresa is null
            ? new DadosLoja("KiVenda")
            : new DadosLoja(
                empresa.NomeComercial,
                empresa.Nif,
                empresa.Endereco,
                empresa.Municipio,
                empresa.Provincia,
                empresa.Telefone,
                empresa.Website,
                empresa.Logo,
                empresa.LogoMimeType);

        var servicoImpressao = scope.ServiceProvider.GetRequiredService<IServicoImpressao>();
        await servicoImpressao.ImprimirReciboVendaAsync(recibo, dadosLoja);

        return true;
    }

    private async void OnCodigoLido(string codigo)
    {
        if (DataContext is not VendasViewModel vm)
        {
            return;
        }

        await vm.ProcessarLeituraScannerAsync(codigo);

        if (_servicoScanner?.ConfiguracaoAtual.AbrirQuantidadeAposLeitura == true &&
            QuantidadeScannerTextBox.IsVisible)
        {
            QuantidadeScannerTextBox.Focus();
            QuantidadeScannerTextBox.SelectAll();
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
