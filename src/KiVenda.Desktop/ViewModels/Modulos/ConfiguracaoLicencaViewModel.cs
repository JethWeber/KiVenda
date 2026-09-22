using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using WeberTech.Licensing.Enums;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoLicencaViewModel : ViewModelBase
{
    public const string ProductId = "kivenda.desktop_v03";

    [ObservableProperty] private LicenseStatus _estado = LicenseStatus.NotFound;
    [ObservableProperty] private Bitmap? _qrCode;
    [ObservableProperty] private string _mensagem = string.Empty;
    [ObservableProperty] private string _cliente = "-";
    [ObservableProperty] private string _plano = "MVP";
    [ObservableProperty] private string _validade = "-";
    [ObservableProperty] private string _diasRestantes = "-";
    [ObservableProperty] private bool _aCarregar;

    public bool LicencaValida => Estado == LicenseStatus.Valid;
    public bool LicencaExpirada => Estado == LicenseStatus.Expired;
    public bool PrecisaAtivacao => Estado is LicenseStatus.NotFound or LicenseStatus.Invalid or LicenseStatus.ProductMismatch or LicenseStatus.MachineMismatch;
    public bool PodeImportar => Estado != LicenseStatus.Valid;
    public bool MostrarQr => PrecisaAtivacao && QrCode is not null;
    public bool MostrarDetalhes => LicencaValida || LicencaExpirada;
    public bool PlataformaSuportada => OperatingSystem.IsWindows();

    public ConfiguracaoLicencaViewModel()
    {
        Atualizar();
    }

    public void Atualizar()
    {
        ACarregar = true;
        Mensagem = string.Empty;

        try
        {
            if (!OperatingSystem.IsWindows())
            {
                Estado = LicenseStatus.NotFound;
                Mensagem = "O licenciamento do KiVenda usa Machine ID via WMI e é validado no Windows. O SDK está integrado, mas a ativação real deve ser feita no Windows.";
                QrCode = null;
                NotificarEstado();
                return;
            }

            Licensing.Initialize(ProductType.KiVenda, ProductId);
            Estado = Licensing.CurrentStatus;
            AplicarInfo();
            GerarQrSeNecessario();
        }
        catch (Exception ex)
        {
            Estado = LicenseStatus.Invalid;
            QrCode = null;
            Mensagem = $"Não foi possível inicializar o licenciamento: {ex.Message}";
            NotificarEstado();
        }
        finally
        {
            ACarregar = false;
        }
    }

    public async Task ImportarLicencaAsync(string caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return;

        ACarregar = true;
        Mensagem = string.Empty;

        try
        {
            if (!OperatingSystem.IsWindows())
            {
                Mensagem = "A ativação real do KiVenda deve ser feita no Windows.";
                return;
            }

            if (Licensing.CurrentStatus == LicenseStatus.NotFound)
            {
                Licensing.Initialize(ProductType.KiVenda, ProductId);
            }

            Estado = Licensing.ImportLicenseFile(caminho);
            AplicarInfo();

            if (Estado == LicenseStatus.Valid)
            {
                Mensagem = "Licença ativada com sucesso. O KiVenda está totalmente desbloqueado.";
                QrCode = null;
            }
            else
            {
                Mensagem = MensagemParaEstado(Estado);
                GerarQrSeNecessario();
            }

            NotificarEstado();
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível importar a licença: {ex.Message}";
        }
        finally
        {
            ACarregar = false;
        }

        await Task.CompletedTask;
    }

    private void AplicarInfo()
    {
        var info = Licensing.GetLicenseInfo();

        Cliente = info?.CustomerName ?? "-";
        Plano = info?.Plan ?? "MVP";

        if (info?.ExpiresAt is { } expiresAt)
        {
            Validade = expiresAt.ToLocalTime().ToString("dd/MM/yyyy");
            DiasRestantes = Licensing.DaysUntilExpiration().ToString();
        }
        else
        {
            Validade = "Perpétua";
            DiasRestantes = "∞";
        }

        NotificarEstado();
    }

    private void GerarQrSeNecessario()
    {
        QrCode = null;

        if (!PrecisaAtivacao || !OperatingSystem.IsWindows())
        {
            OnPropertyChanged(nameof(MostrarQr));
            return;
        }

        try
        {
            byte[] png = Licensing.GenerateActivationQrCode();
            using var stream = new MemoryStream(png);
            QrCode = new Bitmap(stream);
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível gerar o QR de ativação: {ex.Message}";
        }

        OnPropertyChanged(nameof(MostrarQr));
    }

    private void NotificarEstado()
    {
        OnPropertyChanged(nameof(LicencaValida));
        OnPropertyChanged(nameof(LicencaExpirada));
        OnPropertyChanged(nameof(PrecisaAtivacao));
        OnPropertyChanged(nameof(PodeImportar));
        OnPropertyChanged(nameof(MostrarQr));
        OnPropertyChanged(nameof(MostrarDetalhes));
    }

    private static string MensagemParaEstado(LicenseStatus status) => status switch
    {
        LicenseStatus.Invalid => "O ficheiro de licença é inválido ou foi adulterado.",
        LicenseStatus.ProductMismatch => "A licença não pertence ao KiVenda.",
        LicenseStatus.MachineMismatch => "A licença pertence a outra máquina.",
        LicenseStatus.Expired => "A licença expirou. Importe a renovação emitida pela Weber Tech.",
        _ => "A licença não está ativa."
    };
}
