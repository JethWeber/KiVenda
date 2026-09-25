using CommunityToolkit.Mvvm.ComponentModel;
using KiVenda.Desktop.Tema;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ConfiguracaoTemaViewModel : ViewModelBase
{
    private readonly ServicoTema _servicoTema;

    public IReadOnlyList<string> OpcoesTema { get; } =
        ["Sistema", "KiVenda Dark", "KiVenda Light"];

    [ObservableProperty]
    private string _temaSelecionado = "Sistema";

    public string TemaAtualDescricao => _servicoTema.TemaAtual switch
    {
        TemaKiVenda.Dark => "KiVenda Dark",
        TemaKiVenda.Light => "KiVenda Light",
        _ => "Sistema"
    };

    public ConfiguracaoTemaViewModel(ServicoTema servicoTema)
    {
        _servicoTema = servicoTema;
        _temaSelecionado = TemaAtualDescricao;
    }

    partial void OnTemaSelecionadoChanged(string value)
    {
        var tema = value switch
        {
            "KiVenda Dark" => TemaKiVenda.Dark,
            "KiVenda Light" => TemaKiVenda.Light,
            _ => TemaKiVenda.Sistema
        };

        _servicoTema.DefinirTema(tema);
        OnPropertyChanged(nameof(TemaAtualDescricao));
    }
}