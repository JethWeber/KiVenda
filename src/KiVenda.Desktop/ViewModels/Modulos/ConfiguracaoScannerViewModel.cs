using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Infrastructure.Configuracao;
using KiVenda.Infrastructure.Scanner;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoScannerViewModel : ViewModelBase
{
    private readonly IArmazenamentoConfiguracaoLocal _armazenamento;
    private readonly IServicoScanner _scanner;

    [ObservableProperty] private bool _ativo;
    [ObservableProperty] private bool _emitirSomAoLer;
    [ObservableProperty] private bool _adicionarAutomaticamente;
    [ObservableProperty] private bool _abrirQuantidadeAposLeitura;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private bool _aGuardar;
    [ObservableProperty] private string _mensagem = string.Empty;

    public ConfiguracaoScannerViewModel()
    {
        _armazenamento = App.Services.GetRequiredService<IArmazenamentoConfiguracaoLocal>();
        _scanner = App.Services.GetRequiredService<IServicoScanner>();
        _ = CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = string.Empty;
        try
        {
            await _scanner.RecarregarConfiguracaoAsync();
            Aplicar(_scanner.ConfiguracaoAtual);
        }
        catch
        {
            Aplicar(ConfiguracaoScanner.Padrao);
            Mensagem = "Não foi possível ler a configuração anterior. Foram carregados os valores padrão.";
        }
        finally { ACarregar = false; }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        AGuardar = true;
        Mensagem = string.Empty;
        try
        {
            var configuracao = new ConfiguracaoScanner(
                Ativo, EmitirSomAoLer, AdicionarAutomaticamente, AbrirQuantidadeAposLeitura);

            await _armazenamento.GuardarAsync(ConfiguracaoScanner.Chave, configuracao);
            await _scanner.RecarregarConfiguracaoAsync();
            _scanner.Reset();
            Mensagem = "Configuração do scanner guardada e aplicada imediatamente.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao guardar configuração: {ex.Message}";
        }
        finally { AGuardar = false; }
    }

    [RelayCommand]
    private async Task RestaurarPadraoAsync()
    {
        Aplicar(ConfiguracaoScanner.Padrao);
        await GuardarAsync();
    }

    private void Aplicar(ConfiguracaoScanner configuracao)
    {
        Ativo = configuracao.Ativo;
        EmitirSomAoLer = configuracao.EmitirSomAoLer;
        AdicionarAutomaticamente = configuracao.AdicionarAutomaticamente;
        AbrirQuantidadeAposLeitura = configuracao.AbrirQuantidadeAposLeitura;
    }
}
