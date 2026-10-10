using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracoesView : UserControl
{
    // URLs vazias escondem os respetivos links do rodapé.
    private static readonly string SuporteUrl = "";
    private static readonly string DocumentacaoUrl = "";

    private const int AbaEmpresa = 0;
    private const int AbaDispositivos = 1;
    private const int AbaLicenca = 2;

    public ConfiguracoesView()
    {
        InitializeComponent();

        VersaoText.Text = $"v{ObterVersao()}";
        SuporteLink.IsVisible = !string.IsNullOrWhiteSpace(SuporteUrl);
        DocumentacaoLink.IsVisible = !string.IsNullOrWhiteSpace(DocumentacaoUrl);

        AtualizarBotaoGuardar();
    }

    private void Abas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Ignorar eventos de ComboBoxes dentro das abas.
        if (!ReferenceEquals(e.Source, Abas))
            return;

        // Quando a licença restringe o acesso, manter apenas a aba Licença.
        if (DataContext is ConfiguracoesViewModel vm &&
            vm.SomenteLicenca &&
            Abas.SelectedIndex != AbaLicenca)
        {
            Abas.SelectedIndex = AbaLicenca;
            return;
        }

        AtualizarBotaoGuardar();
    }

    private void AtualizarBotaoGuardar()
    {
        if (GuardarBtn is null || Abas is null)
            return;

        GuardarBtn.IsVisible =
            Abas.SelectedIndex is AbaEmpresa or AbaDispositivos;
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ConfiguracoesViewModel vm)
            return;

        switch (Abas.SelectedIndex)
        {
            case AbaEmpresa:
                if (vm.Empresa.PodeExecutar)
                    await vm.Empresa.GuardarCommand.ExecuteAsync(null);
                break;

            case AbaDispositivos:
                if (!vm.Scanner.AGuardar)
                    await vm.Scanner.GuardarCommand.ExecuteAsync(null);

                if (!vm.Impressora.AGuardar)
                    await vm.Impressora.GuardarCommand.ExecuteAsync(null);
                break;

            // A licença é importada no seu painel.
            // O tema é aplicado imediatamente.
            case AbaLicenca:
            case 4:
                break;
        }
    }

    private void Suporte_Click(object? sender, RoutedEventArgs e)
        => Abrir(SuporteUrl);

    private void Documentacao_Click(object? sender, RoutedEventArgs e)
        => Abrir(DocumentacaoUrl);

    private static void Abrir(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // Nenhum navegador ou handler disponível.
        }
    }

    private static string ObterVersao()
    {
        var versao = Assembly.GetEntryAssembly()?.GetName().Version;

        return versao is null
            ? "1.0.0"
            : $"{versao.Major}.{versao.Minor}.{versao.Build}";
    }
}
