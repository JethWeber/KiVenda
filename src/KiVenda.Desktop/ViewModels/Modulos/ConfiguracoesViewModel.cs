namespace KiVenda.Desktop.ViewModels.Modulos;

public sealed class ConfiguracoesViewModel : ViewModelBase
{
    public ConfiguracaoScannerViewModel Scanner { get; } = new();
    public ConfiguracaoLicencaViewModel Licenca { get; } = new();
}
