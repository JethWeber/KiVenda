using Avalonia.Controls;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.Views.Shell;

public partial class ShellView : UserControl
{
    private ShellViewModel? _viewModel;

    public ShellView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.MeusDadosSolicitados -= AbrirMeusDados;

        _viewModel = DataContext as ShellViewModel;

        if (_viewModel is not null)
            _viewModel.MeusDadosSolicitados += AbrirMeusDados;
    }

    private async void AbrirMeusDados(object? sender, System.EventArgs e)
    {
        if (_viewModel is null)
            return;

        var janela = new MeusDadosWindow
        {
            DataContext = new MeusDadosViewModel(App.Services.GetRequiredService<SessaoUtilizadorAtual>())
        };

        if (TopLevel.GetTopLevel(this) is Window owner)
            janela.WindowStartupLocation = WindowStartupLocation.CenterOwner;

        if (janela.DataContext is MeusDadosViewModel vm)
        {
            vm.Cancelado += (_, _) => janela.Close();
            vm.GuardadoComSucesso += (_, _) =>
            {
                janela.Close();
                _viewModel.TerminarSessaoCommand.Execute(null);
            };
        }

        if (TopLevel.GetTopLevel(this) is Window ownerWindow)
            await janela.ShowDialog(ownerWindow);
        else
            janela.Show();
    }
}
