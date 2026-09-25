using Avalonia.Controls;
using Avalonia.Platform.Storage;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracaoEmpresaView : UserControl
{
    public ConfiguracaoEmpresaView() => InitializeComponent();

    private async void EscolherLogo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null || DataContext is not ConfiguracaoEmpresaViewModel vm)
            return;

        var ficheiros = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Selecionar logótipo da empresa",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Imagens")
                {
                    Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp"]
                }
            ]
        });

        var ficheiro = ficheiros.FirstOrDefault();
        if (ficheiro is null)
            return;

        await using var stream = await ficheiro.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);

        var bytes = memory.ToArray();
        var mime = ficheiro.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png"
            : ficheiro.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ? "image/webp"
            : "image/jpeg";

        if (bytes.Length > 2 * 1024 * 1024)
        {
            vm.DefinirLogo(Array.Empty<byte>(), mime);
            vm.RemoverLogoLocal();
            return;
        }

        vm.DefinirLogo(bytes, mime);
    }

    private void RemoverLogo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ConfiguracaoEmpresaViewModel vm)
            vm.RemoverLogoLocal();
    }
}
