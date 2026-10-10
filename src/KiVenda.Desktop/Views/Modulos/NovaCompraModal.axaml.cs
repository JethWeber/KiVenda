using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class NovaCompraModal : UserControl
{
    public NovaCompraModal()
    {
        InitializeComponent();

        Overlay.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty && Overlay.IsVisible)
                Dispatcher.UIThread.Post(() => CampoPrincipal.Focus());
        };
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is ComprasViewModel vm)
        {
            vm.FecharFormularioCommand.Execute(null);
            e.Handled = true;
        }
    }
}
