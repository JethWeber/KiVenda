using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Desktop.Tema;
using KiVenda.Infrastructure.Backup;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracoesViewModel : ViewModelBase
{
    public ConfiguracaoEmpresaViewModel Empresa { get; } = new();
    public ConfiguracaoScannerViewModel Scanner { get; } = new();
    public ConfiguracaoImpressoraViewModel Impressora { get; } = new();
    public ConfiguracaoLicencaViewModel Licenca { get; } = new();
    public ConfiguracaoBackupViewModel Backup { get; }
    public ConfiguracaoTemaViewModel Tema { get; }

    [ObservableProperty]
    private int _indiceAbaSelecionada;

    [ObservableProperty]
    private bool _somenteLicenca;

    public bool AbasGeraisVisiveis => !SomenteLicenca;

    partial void OnSomenteLicencaChanged(bool value) =>
        OnPropertyChanged(nameof(AbasGeraisVisiveis));

    public ConfiguracoesViewModel(IServicoBackup servicoBackup, ServicoTema servicoTema)
    {
        Backup = new ConfiguracaoBackupViewModel(servicoBackup);
        Tema = new ConfiguracaoTemaViewModel(servicoTema);
    }

    public void SelecionarLicenca() => IndiceAbaSelecionada = 2;

    public void BloquearNasLicenca()
    {
        SomenteLicenca = true;
        IndiceAbaSelecionada = 2;
    }
}
