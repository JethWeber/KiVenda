using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracoesView : UserControl
{
    public ConfiguracoesView() => InitializeComponent();

    private void Abas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is ConfiguracoesViewModel vm &&
            vm.SomenteLicenca &&
            Abas.SelectedIndex != 2)
        {
            Abas.SelectedIndex = 2;
        }
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ConfiguracoesViewModel vm)
            return;

        switch (Abas.SelectedIndex)
        {
            case 0:
                await vm.Empresa.GuardarCommand.ExecuteAsync(null);
                break;
            case 1:
                await vm.Scanner.GuardarCommand.ExecuteAsync(null);
                await vm.Impressora.GuardarCommand.ExecuteAsync(null);
                break;
            // A licença é importada pelo respetivo painel e o tema é aplicado
            // imediatamente quando o utilizador escolhe uma opção.
            case 2:
            case 4:
                break;
        }
    }

    private async void Suporte_Click(object? sender, RoutedEventArgs e)
    {
        await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(
            new Uri("https://github.com/JethWeber/KiVenda/issues"));
    }

    private async void Documentacao_Click(object? sender, RoutedEventArgs e)
    {
        await TopLevel.GetTopLevel(this)!.Launcher.LaunchUriAsync(
            new Uri("https://github.com/JethWeber/KiVenda"));
    }
}
