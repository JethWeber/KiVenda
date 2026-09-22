using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracaoLicencaView : UserControl
{
    public ConfiguracaoLicencaView() => InitializeComponent();

    private async void ImportarLicenca_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var ficheiros = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecionar licença do KiVenda",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Licença Weber Tech")
                {
                    Patterns = ["*.wta"]
                }
            ]
        });

        var ficheiro = ficheiros.FirstOrDefault();
        if (ficheiro is null)
            return;

        if (DataContext is ConfiguracaoLicencaViewModel vm)
        {
            await vm.ImportarLicencaAsync(ficheiro.Path.LocalPath);
        }
    }
}
