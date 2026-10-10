using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using KiVenda.Desktop.ViewModels.Modulos;

namespace KiVenda.Desktop.Views.Modulos;

public partial class ConfiguracaoEmpresaView : UserControl
{
    private Bitmap? _logoBitmap;
    private INotifyPropertyChanged? _vmObservado;
    private PropertyInfo? _logoBytesProp;

    public ConfiguracaoEmpresaView() => InitializeComponent();

    // ── Pré-visualização ─────────────────────────────────────────────

    private void MostrarPreview(byte[]? bytes)
    {
        var anterior = _logoBitmap;
        _logoBitmap = null;

        if (bytes is { Length: > 0 })
        {
            try
            {
                using var ms = new MemoryStream(bytes);
                _logoBitmap = new Bitmap(ms);
            }
            catch
            {
                _logoBitmap = null; // imagem inválida: mostra o estado vazio
            }
        }

        LogoPreview.Source = _logoBitmap;
        LogoPreview.IsVisible = _logoBitmap is not null;
        LogoPlaceholder.IsVisible = _logoBitmap is null;

        anterior?.Dispose();
    }

    // Se o ViewModel guardar o logótipo num byte[] (nome com "Logo"),
    // a pré-visualização também mostra o logótipo já gravado e acompanha
    // "Remover cadastro". Quando souber o nome exato da propriedade, pode
    // trocar esta procura por um acesso direto.
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_vmObservado is not null)
            _vmObservado.PropertyChanged -= Vm_PropertyChanged;

        _vmObservado = DataContext as INotifyPropertyChanged;
        _logoBytesProp = DataContext?.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.PropertyType == typeof(byte[])
                                 && p.Name.Contains("Logo", StringComparison.OrdinalIgnoreCase));

        if (_vmObservado is not null)
            _vmObservado.PropertyChanged += Vm_PropertyChanged;

        AtualizarPreviewDoVm();
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_logoBytesProp is null)
            return;

        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == _logoBytesProp.Name)
            Dispatcher.UIThread.Post(AtualizarPreviewDoVm);
    }

    private void AtualizarPreviewDoVm()
    {
        if (_logoBytesProp is null || DataContext is null)
            return;

        MostrarPreview(_logoBytesProp.GetValue(DataContext) as byte[]);
    }

    // ── Ações ────────────────────────────────────────────────────────

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
            MostrarPreview(null);
            return;
        }

        vm.DefinirLogo(bytes, mime);
        MostrarPreview(bytes);
    }

    private void RemoverLogo_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ConfiguracaoEmpresaViewModel vm)
            vm.RemoverLogoLocal();

        MostrarPreview(null);
    }
}
