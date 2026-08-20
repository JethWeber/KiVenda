using CommunityToolkit.Mvvm.ComponentModel;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// ViewModel provisório da janela principal, usado apenas como prova de
/// vida da Fase 0 (solução compilável + janela Avalonia a abrir).
/// Será substituído pela shell definitiva (menu lateral + navegação
/// entre módulos) na Fase 6.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "KiVenda Desktop — Fase 0 concluída: fundação do projeto pronta.";

    [ObservableProperty]
    private string _versao = "v0.0.1-fase0";
}
