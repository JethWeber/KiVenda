using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Infrastructure.Configuracao;
using KiVenda.Infrastructure.Impressao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoImpressoraViewModel : ViewModelBase
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;
    private readonly IDetectorImpressoras _detector;
    private readonly IServicoImpressaoTermica _servicoImpressao;

    public ObservableCollection<DispositivoImpressora> Dispositivos { get; } = [];

    public IReadOnlyList<OpcaoConexaoImpressora> TiposConexao { get; } =
        OperatingSystem.IsWindows()
            ? new[]
            {
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.WindowsSpooler,
                    "Impressora instalada",
                    "Usa o spooler do Windows; ideal para USB, rede ou impressoras instaladas pelo sistema."),
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.Rede,
                    "Rede (TCP/IP)",
                    "Liga diretamente ao IP/hostname da impressora, normalmente na porta 9100."),
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.Serial,
                    "Serial / USB-Serial",
                    "Usa uma porta COM disponibilizada pela impressora ou adaptador.")
            }
            : new[]
            {
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.DispositivoLocal,
                    "USB / dispositivo Linux",
                    "Usa diretamente /dev/usb/lp* ou outro dispositivo de impressão."),
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.Rede,
                    "Rede (TCP/IP)",
                    "Liga diretamente ao IP/hostname da impressora, normalmente na porta 9100."),
                new OpcaoConexaoImpressora(
                    TipoConexaoImpressora.Serial,
                    "Serial / USB-Serial",
                    "Usa /dev/ttyUSB*, /dev/ttyACM* ou /dev/serial/by-id/*.")
            };

    [ObservableProperty] private TipoConexaoImpressora _tipoConexao;
    [ObservableProperty] private bool _ativo;
    [ObservableProperty] private string _dispositivo = string.Empty;
    [ObservableProperty] private DispositivoImpressora? _dispositivoSelecionado;
    [ObservableProperty] private string _enderecoRede = string.Empty;
    [ObservableProperty] private int _portaRede;
    [ObservableProperty] private int _baudRate;
    [ObservableProperty] private int _colunas;
    [ObservableProperty] private int _linhasAlimentacaoFinal;
    [ObservableProperty] private bool _cortarPapel;
    [ObservableProperty] private string _encodingNome = string.Empty;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private bool _aGuardar;
    [ObservableProperty] private bool _aDetetar;
    [ObservableProperty] private bool _aTestar;
    [ObservableProperty] private string _mensagem = string.Empty;
    [ObservableProperty] private string _estadoDeteccao = "A procurar dispositivos...";

    public bool EhRede => TipoConexao == TipoConexaoImpressora.Rede;
    public bool EhSerial => TipoConexao == TipoConexaoImpressora.Serial;
    public bool EhConexaoLocal =>
        TipoConexao is TipoConexaoImpressora.WindowsSpooler or TipoConexaoImpressora.DispositivoLocal or TipoConexaoImpressora.Serial;

    public ConfiguracaoImpressoraViewModel()
    {
        _armazenamento = App.Services.GetRequiredService<IArmazenamentoConfiguracaoLocal>();
        _detector = App.Services.GetRequiredService<IDetectorImpressoras>();
        _servicoImpressao = App.Services.GetRequiredService<IServicoImpressaoTermica>();
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        ACarregar = true;

        try
        {
            var configuracao = await _armazenamento.ObterAsync<ConfiguracaoImpressoraTermica>(
                ConfiguracaoImpressoraTermica.Chave);

            Aplicar((configuracao ?? ConfiguracaoImpressoraTermica.Padrao).NormalizarParaAmbiente());
            await ProcurarAsync();
        }
        catch (Exception ex)
        {
            Aplicar(ConfiguracaoImpressoraTermica.Padrao);
            Mensagem = $"Não foi possível carregar a configuração: {ex.Message}";
        }
        finally
        {
            ACarregar = false;
        }
    }

    [RelayCommand]
    private async Task ProcurarAsync()
    {
        if (ADetetar || TipoConexao == TipoConexaoImpressora.Rede)
            return;

        ADetetar = true;
        Mensagem = string.Empty;
        EstadoDeteccao = "A procurar dispositivos...";

        try
        {
            var encontrados = await _detector.DetetarAsync();

            Dispositivos.Clear();
            foreach (var dispositivo in encontrados.Where(d => d.TipoConexao == TipoConexao))
                Dispositivos.Add(dispositivo);

            var correspondencia = Dispositivos.FirstOrDefault(d =>
                string.Equals(d.Id, Dispositivo, StringComparison.OrdinalIgnoreCase));

            var disponiveis = Dispositivos.Where(d => d.Disponivel).ToList();

            DispositivoSelecionado = correspondencia ??
                (disponiveis.Count == 1 ? disponiveis[0] : null);

            EstadoDeteccao = Dispositivos.Count == 0
                ? "Nenhum dispositivo compatível foi detetado."
                : $"{disponiveis.Count} dispositivo(s) disponível(is).";
        }
        catch (Exception ex)
        {
            EstadoDeteccao = "Falha na deteção.";
            Mensagem = $"Não foi possível procurar dispositivos: {ex.Message}";
        }
        finally
        {
            ADetetar = false;
        }
    }

    [RelayCommand]
    private async Task TestarAsync()
    {
        if (ATestar)
            return;

        try
        {
            var configuracao = CriarConfiguracao();
            configuracao.Validar();

            ATestar = true;
            Mensagem = "A enviar teste ESC/POS...";

            await _servicoImpressao.TestarImpressoraAsync(configuracao);

            Mensagem = "Teste enviado com sucesso. Verifique a impressora.";
        }
        catch (UnauthorizedAccessException)
        {
            Mensagem = "A impressora existe, mas o KiVenda não tem permissão para escrever nela.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Falha no teste da impressora: {ex.Message}";
        }
        finally
        {
            ATestar = false;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;

        try
        {
            var configuracao = CriarConfiguracao();
            configuracao.Validar();

            await _armazenamento.GuardarAsync(
                ConfiguracaoImpressoraTermica.Chave,
                configuracao);

            Aplicar(configuracao);
            Mensagem = "Configuração da impressora guardada localmente.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao guardar configuração: {ex.Message}";
        }
        finally
        {
            AGuardar = false;
        }
    }

    [RelayCommand]
    private async Task RestaurarPadraoAsync()
    {
        Aplicar(ConfiguracaoImpressoraTermica.Padrao);
        DispositivoSelecionado = null;
        await GuardarAsync();
        await ProcurarAsync();
    }

    partial void OnTipoConexaoChanged(TipoConexaoImpressora value)
    {
        DispositivoSelecionado = null;
        Dispositivo = string.Empty;
        EnderecoRede = string.Empty;

        OnPropertyChanged(nameof(EhRede));
        OnPropertyChanged(nameof(EhSerial));
        OnPropertyChanged(nameof(EhConexaoLocal));

        _ = ProcurarAsync();
    }

    partial void OnDispositivoSelecionadoChanged(DispositivoImpressora? value)
    {
        if (value is not null)
            Dispositivo = value.Id;
    }

    partial void OnDispositivoChanged(string value)
    {
        if (DispositivoSelecionado is not null &&
            !string.Equals(DispositivoSelecionado.Id, value, StringComparison.OrdinalIgnoreCase))
            DispositivoSelecionado = null;
    }

    private ConfiguracaoImpressoraTermica CriarConfiguracao() =>
        new(
            TipoConexao,
            Dispositivo.Trim(),
            EnderecoRede.Trim(),
            PortaRede > 0 ? PortaRede : 9100,
            BaudRate > 0 ? BaudRate : 115200,
            Colunas > 0 ? Colunas : 48,
            LinhasAlimentacaoFinal >= 0 ? LinhasAlimentacaoFinal : 4,
            CortarPapel,
            string.IsNullOrWhiteSpace(EncodingNome) ? "cp850" : EncodingNome.Trim(),
            Ativo);

    private void Aplicar(ConfiguracaoImpressoraTermica configuracao)
    {
        var normalizada = configuracao.NormalizarParaAmbiente();

        TipoConexao = normalizada.TipoConexao;
        Ativo = normalizada.Ativo;
        Dispositivo = normalizada.Dispositivo;
        EnderecoRede = normalizada.EnderecoRede;
        PortaRede = normalizada.PortaRede;
        BaudRate = normalizada.BaudRate;
        Colunas = normalizada.Colunas;
        LinhasAlimentacaoFinal = normalizada.LinhasAlimentacaoFinal;
        CortarPapel = normalizada.CortarPapel;
        EncodingNome = normalizada.EncodingNome;
    }
}
