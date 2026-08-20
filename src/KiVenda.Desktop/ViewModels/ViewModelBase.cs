using CommunityToolkit.Mvvm.ComponentModel;

namespace KiVenda.Desktop.ViewModels;

/// <summary>
/// Base comum a todos os ViewModels do KiVenda Desktop.
/// Usa CommunityToolkit.Mvvm (ObservableObject) para geração de
/// propriedades observáveis via [ObservableProperty] e comandos via
/// [RelayCommand] nas classes derivadas.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}
