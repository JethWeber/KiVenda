using Avalonia.Controls;
using Avalonia.Interactivity;
using KiVenda.Application.Produtos;
using KiVenda.Application.Vendas;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class VendasView : UserControl
{
    public VendasView()
    {
        InitializeComponent();
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
