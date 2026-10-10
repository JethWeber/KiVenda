using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Desktop.Autenticacao;
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

    // Uma licença expirada também precisa de mostrar o fluxo de renovação.
    // Caso contrário, o botão de importar .wta fica escondido pelo painel pai.
    public bool PrecisaAtivacao => Estado is
        LicenseStatus.NotFound or
        LicenseStatus.Invalid or
        LicenseStatus.ProductMismatch or
        LicenseStatus.MachineMismatch or
        LicenseStatus.Expired;

    public bool PodeImportar => Estado != LicenseStatus.Valid;
    public bool MostrarQr => PrecisaAtivacao && QrCode is not null;
    public bool MostrarDetalhes => LicencaValida || LicencaExpirada;
    public bool PlataformaSuportada => OperatingSystem.IsWindows() || OperatingSystem.IsLinux();

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
            if (Licensing.GetLicensePath() is null)
            {
                Licensing.Initialize(ProductType.KiVenda, ProductId);
            }

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
        var politica = PoliticaLicencaKiVenda.Avaliar();

        if (politica.Acesso is AcessoLicenca.Tolerancia or AcessoLicenca.Bloqueado)
            Estado = LicenseStatus.Expired;

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

        if (politica.MostrarBanner)
            Mensagem = politica.Mensagem;

        NotificarEstado();
    }

    private void GerarQrSeNecessario()
    {
        QrCode = null;

        if (!PrecisaAtivacao || !PlataformaSuportada)
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
