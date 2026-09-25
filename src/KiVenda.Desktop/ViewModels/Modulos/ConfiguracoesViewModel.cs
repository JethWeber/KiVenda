using KiVenda.Infrastructure.Backup;

namespace KiVenda.Desktop.ViewModels.Modulos;

public sealed class ConfiguracoesViewModel : ViewModelBase
{
    public ConfiguracaoEmpresaViewModel Empresa { get; } = new();
    public ConfiguracaoScannerViewModel Scanner { get; } = new();
    public ConfiguracaoLicencaViewModel Licenca { get; } = new();
    public ConfiguracaoBackupViewModel Backup { get; }

    public ConfiguracoesViewModel(IServicoBackup servicoBackup)
    {
        Backup = new ConfiguracaoBackupViewModel(servicoBackup);
    }
}
