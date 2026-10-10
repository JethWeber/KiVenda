using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracoesView : UserControl
{
    // Preencha para mostrar as ligações do rodapé. Vazio = botão escondido.
    private static readonly string SuporteUrl = "";
    private static readonly string DocumentacaoUrl = "";

    // Separadores onde o botão "Guardar alterações" faz sentido.
    private const int AbaEmpresa = 0;
    private const int AbaDispositivos = 1;

    public ConfiguracoesView()
    {
        InitializeComponent();

        VersaoText.Text = $"v{ObterVersao()}";
        SuporteLink.IsVisible = !string.IsNullOrWhiteSpace(SuporteUrl);
        DocumentacaoLink.IsVisible = !string.IsNullOrWhiteSpace(DocumentacaoUrl);
    }

    private void Abas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // SelectionChanged de ComboBox filhos também sobe até aqui.
        if (!ReferenceEquals(e.Source, Abas) || GuardarBtn is null)
            return;

        GuardarBtn.IsVisible = Abas.SelectedIndex is AbaEmpresa or AbaDispositivos;
    }

    private void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ConfiguracoesViewModel vm)
            return;

        switch (Abas.SelectedIndex)
        {
            case AbaEmpresa:
                if (vm.Empresa.PodeExecutar)
                    Executar(vm.Empresa.GuardarCommand);
                break;

            case AbaDispositivos:
                if (!vm.Scanner.AGuardar)
                    Executar(vm.Scanner.GuardarCommand);
                if (!vm.Impressora.AGuardar)
                    Executar(vm.Impressora.GuardarCommand);
                break;
        }
    }

    private static void Executar(ICommand? comando)
    {
        if (comando?.CanExecute(null) == true)
            comando.Execute(null);
    }

    private void Suporte_Click(object? sender, RoutedEventArgs e) => Abrir(SuporteUrl);

    private void Documentacao_Click(object? sender, RoutedEventArgs e) => Abrir(DocumentacaoUrl);

    private static void Abrir(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Sem navegador/handler disponível: ignora silenciosamente.
        }
    }

    private static string ObterVersao()
    {
        var versao = Assembly.GetEntryAssembly()?.GetName().Version;
        return versao is null ? "1.0.0" : $"{versao.Major}.{versao.Minor}.{versao.Build}";
    }
}
