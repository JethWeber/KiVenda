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

    [ObservableProperty] private bool _ativo;
    [ObservableProperty] private string _dispositivo = string.Empty;
    [ObservableProperty] private DispositivoImpressora? _dispositivoSelecionado;
    [ObservableProperty] private int _colunas;
    [ObservableProperty] private int _linhasAlimentacaoFinal;
    [ObservableProperty] private bool _cortarPapel;
    [ObservableProperty] private string _encodingNome = string.Empty;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private bool _aGuardar;
    [ObservableProperty] private bool _aDetetar;
    [ObservableProperty] private bool _aTestar;
    [ObservableProperty] private string _mensagem = string.Empty;
    [ObservableProperty] private string _estadoDeteccao = "A procurar impressoras...";

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

            Aplicar(configuracao ?? ConfiguracaoImpressoraTermica.Padrao);
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
        if (ADetetar)
        {
            return;
        }

        ADetetar = true;
        Mensagem = string.Empty;
        EstadoDeteccao = "A procurar impressoras...";

        try
        {
            var encontrados = await _detector.DetarAsync();

            Dispositivos.Clear();
            foreach (var dispositivo in encontrados)
            {
                Dispositivos.Add(dispositivo);
            }

            var correspondenciaGuardada = Dispositivos.FirstOrDefault(d =>
                string.Equals(d.Id, Dispositivo, StringComparison.OrdinalIgnoreCase));

            var disponiveisDetetados = Dispositivos
                .Where(d => d.Disponivel)
                .ToList();

            DispositivoSelecionado = correspondenciaGuardada
                ?? (disponiveisDetetados.Count == 1 ? disponiveisDetetados[0] : null);

            if (Dispositivos.Count == 0)
            {
                EstadoDeteccao = "Nenhuma impressora foi detetada.";
            }
            else
            {
                var disponiveis = Dispositivos.Count(d => d.Disponivel);
                EstadoDeteccao = disponiveis == 1
                    ? "1 impressora disponível."
                    : $"{disponiveis} impressoras disponíveis.";

                var semPermissao = Dispositivos.Count(
                    d => d.Estado == EstadoDispositivoImpressora.SemPermissao);

                if (semPermissao > 0)
                {
                    EstadoDeteccao += $" {semPermissao} dispositivo(s) sem permissão de escrita.";
                }
            }
        }
        catch (Exception ex)
        {
            EstadoDeteccao = "Falha na deteção.";
            Mensagem = $"Não foi possível procurar impressoras: {ex.Message}";
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
        {
            return;
        }

        var dispositivo = Dispositivo.Trim();

        if (string.IsNullOrWhiteSpace(dispositivo))
        {
            Mensagem = "Selecione uma impressora para testar.";
            return;
        }

        ATestar = true;
        Mensagem = "A enviar teste ESC/POS...";

        try
        {
            await _servicoImpressao.TestarImpressoraAsync(dispositivo);
            Mensagem = "Teste enviado com sucesso. Verifique a impressora.";
        }
        catch (UnauthorizedAccessException)
        {
            Mensagem = "A impressora foi encontrada, mas o KiVenda não tem permissão para escrever nela.";
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
            var dispositivo = Dispositivo.Trim();

            var configuracao = new ConfiguracaoImpressoraTermica(
                dispositivo,
                Colunas > 0 ? Colunas : 48,
                LinhasAlimentacaoFinal >= 0 ? LinhasAlimentacaoFinal : 4,
                CortarPapel,
                2,
                string.IsNullOrWhiteSpace(EncodingNome) ? "cp850" : EncodingNome.Trim(),
                Ativo);

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

    partial void OnDispositivoSelecionadoChanged(DispositivoImpressora? value)
    {
        if (value is not null)
        {
            Dispositivo = value.Id;
        }
    }

    partial void OnDispositivoChanged(string value)
    {
        if (DispositivoSelecionado is not null &&
            !string.Equals(DispositivoSelecionado.Id, value, StringComparison.OrdinalIgnoreCase))
        {
            DispositivoSelecionado = null;
        }
    }

    private void Aplicar(ConfiguracaoImpressoraTermica configuracao)
    {
        Ativo = configuracao.Ativo;
        Dispositivo = configuracao.Dispositivo;
        Colunas = configuracao.Colunas;
        LinhasAlimentacaoFinal = configuracao.LinhasAlimentacaoFinal;
        CortarPapel = configuracao.CortarPapel;
        EncodingNome = configuracao.EncodingNome;
    }
}
