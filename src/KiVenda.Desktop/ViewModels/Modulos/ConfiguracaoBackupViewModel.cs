using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Infrastructure.Backup;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoBackupViewModel : ViewModelBase
{
    private readonly IServicoBackup _servicoBackup;

    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private string _mensagem = "Pronto para criar ou restaurar um backup.";
    [ObservableProperty] private string _ultimoBackup = "Nenhum backup criado nesta sessão.";

    public bool PodeExecutar => !ACarregar;

    public ConfiguracaoBackupViewModel(IServicoBackup servicoBackup) => _servicoBackup = servicoBackup;

    public async Task CriarBackupAsync(string pastaDestino)
    {
        await ExecutarAsync(async () =>
        {
            var resultado = await _servicoBackup.CriarBackupAsync(pastaDestino);
            UltimoBackup = $"{resultado.DataHora:dd/MM/yyyy HH:mm} · {resultado.TamanhoBytes / 1024.0:N1} KB\n{resultado.CaminhoFicheiro}";
            Mensagem = "Backup criado com sucesso.";
        });
    }

    public async Task RestaurarBackupAsync(string caminho)
    {
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
        OnPropertyChanged(nameof(PodeExecutar));
        try { await acao(); }
        catch (Exception ex) { Mensagem = $"Operação de backup falhou: {ex.Message}"; }
        finally
        {
            ACarregar = false;
            OnPropertyChanged(nameof(PodeExecutar));
        }
    }
}
