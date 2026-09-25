using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Infrastructure.Configuracao;
using KiVenda.Infrastructure.Impressao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoImpressoraViewModel : ViewModelBase
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;

    [ObservableProperty] private bool _ativo;
    [ObservableProperty] private string _dispositivo = string.Empty;
    [ObservableProperty] private int _colunas;
    [ObservableProperty] private int _linhasAlimentacaoFinal;
    [ObservableProperty] private bool _cortarPapel;
    [ObservableProperty] private string _encodingNome = string.Empty;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private bool _aGuardar;
    [ObservableProperty] private string _mensagem = string.Empty;

    public ConfiguracaoImpressoraViewModel()
    {
        _armazenamento = App.Services.GetRequiredService<IArmazenamentoConfiguracaoLocal>();
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = string.Empty;

        try
        {
            var configuracao = await _armazenamento.ObterAsync<ConfiguracaoImpressoraTermica>(
                ConfiguracaoImpressoraTermica.Chave);

            Aplicar(configuracao ?? ConfiguracaoImpressoraTermica.Padrao);
        }
        catch
        {
            Aplicar(ConfiguracaoImpressoraTermica.Padrao);
            Mensagem = "Não foi possível ler a configuração da impressora. Foram carregados os valores padrão.";
        }
        finally
        {
            ACarregar = false;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;

        try
        {
            var configuracao = new ConfiguracaoImpressoraTermica(
                Ativo,
                Dispositivo.Trim(),
                Colunas > 0 ? Colunas : 48,
                LinhasAlimentacaoFinal >= 0 ? LinhasAlimentacaoFinal : 4,
                CortarPapel,
                2,
                string.IsNullOrWhiteSpace(EncodingNome) ? "cp850" : EncodingNome.Trim());

            await _armazenamento.GuardarAsync(
                ConfiguracaoImpressoraTermica.Chave,
                configuracao);

            Aplicar(configuracao);
            Mensagem = "Configuração da impressora guardada no ficheiro local.";
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
        await GuardarAsync();
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
