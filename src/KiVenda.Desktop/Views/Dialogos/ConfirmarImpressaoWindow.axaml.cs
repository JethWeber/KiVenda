using Avalonia.Controls;
using Avalonia.Interactivity;

namespace KiVenda.Desktop.Views.Dialogos;

public partial class ConfirmarImpressaoWindow : Window
{
    public string Preview { get; }

    public ConfirmarImpressaoWindow(string preview)
    {
        Preview = preview;
        DataContext = this;
        InitializeComponent();
    }

    private void Sim_Click(object? sender, RoutedEventArgs e) => Close(true);
    private void Nao_Click(object? sender, RoutedEventArgs e) => Close(false);
}