using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracaoBackupView : UserControl
{
    public ConfiguracaoBackupView() => InitializeComponent();

    private async void CriarBackup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not ConfiguracaoBackupViewModel vm)
            return;

        var pasta = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Escolher pasta para guardar o backup",
            AllowMultiple = false
        });

        var selecionada = pasta.FirstOrDefault();
        if (selecionada is not null)
            await vm.CriarBackupAsync(selecionada.Path.LocalPath);
    }

    private async void RestaurarBackup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not ConfiguracaoBackupViewModel vm)
            return;

        var ficheiros = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecionar backup do KiVenda",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Backup KiVenda")
                {
                    Patterns = ["*.db", "*.sqlite", "*.sqlite3"]
                }
            ]
        });

        var ficheiro = ficheiros.FirstOrDefault();
        if (ficheiro is not null)
            await vm.RestaurarBackupAsync(ficheiro.Path.LocalPath);
    }
}
