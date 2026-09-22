using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Infrastructure.Backup;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoBackupViewModel : ViewModelBase
{
    private readonly IServicoBackup _servicoBackup;

    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private string _mensagem = string.Empty;
    [ObservableProperty] private string _ultimoBackup = "Nenhum backup criado nesta sessão.";
    [ObservableProperty] private string _caminhoRestauracao = string.Empty;

    public bool PodeExecutar => !ACarregar;

    public ConfiguracaoBackupViewModel(IServicoBackup servicoBackup)
    {
        _servicoBackup = servicoBackup;
    }

    [RelayCommand]
    private async Task CriarBackupAsync(string? pastaDestino)
    {
        if (string.IsNullOrWhiteSpace(pastaDestino))
            return;

        await ExecutarAsync(async () =>
        {
            var resultado = await _servicoBackup.CriarBackupAsync(pastaDestino);
            UltimoBackup = $"{resultado.DataHora:dd/MM/yyyy HH:mm} · {resultado.TamanhoBytes / 1024.0:N1} KB\n{resultado.CaminhoFicheiro}";
            Mensagem = "Backup criado com sucesso.";
        });
    }

    [RelayCommand]
    private async Task RestaurarBackupAsync(string? caminho)
    {
        if (string.IsNullOrWhiteSpace(caminho))
            return;

        await ExecutarAsync(async () =>
        {
            if (!await _servicoBackup.ValidarFicheiroBackupAsync(caminho))
            {
                Mensagem = "O ficheiro selecionado não é um backup SQLite válido.";
                return;
            }

            await _servicoBackup.RestaurarBackupAsync(caminho);
            Mensagem = "Backup restaurado com sucesso. Reinicie o KiVenda para aplicar a base de dados restaurada.";
        });
    }

    private async Task ExecutarAsync(Func<Task> acao)
    {
        ACarregar = true;
        Mensagem = string.Empty;
        OnPropertyChanged(nameof(PodeExecutar));

        try
        {
            await acao();
        }
        catch (Exception ex)
        {
            Mensagem = $"Operação de backup falhou: {ex.Message}";
        }
        finally
        {
            ACarregar = false;
            OnPropertyChanged(nameof(PodeExecutar));
        }
    }
}
