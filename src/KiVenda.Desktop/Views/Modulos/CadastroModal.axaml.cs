using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class CadastroModal : UserControl
{
    public CadastroModal()
    {
        InitializeComponent();

        // Ao abrir, põe o foco no primeiro campo (também permite que Esc funcione logo).
        Overlay.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsVisibleProperty && Overlay.IsVisible)
                Dispatcher.UIThread.Post(() => CampoPrincipal.Focus());
        };
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is CadastrosViewModel vm)
        {
            vm.FecharFormularioCommand.Execute(null);
            e.Handled = true;
        }
    }
}
