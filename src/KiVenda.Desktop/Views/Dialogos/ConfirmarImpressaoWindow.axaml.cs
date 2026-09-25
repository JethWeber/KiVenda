using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KiVenda.Desktop.Views.Dialogos;

public partial class ConfirmarImpressaoWindow : Window
{
    public ConfirmarImpressaoWindow()
    {
        InitializeComponent();
    }

    private void Sim_Click(object? sender, RoutedEventArgs e) => Close(true);

    private void Nao_Click(object? sender, RoutedEventArgs e) => Close(false);
}