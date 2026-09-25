using System.Collections.ObjectModel;

namespace KiVenda.Desktop.Notificacoes;

public sealed class ServicoNotificacoes
{
    public ObservableCollection<Notificacao> Notificacoes { get; } = new();

    public event EventHandler? Alteradas;

    public int NaoLidas => Notificacoes.Count(x => !x.Lida);

    public void Adicionar(string titulo, string mensagem)
    {
        Notificacoes.Insert(0, new Notificacao(titulo, mensagem, DateTime.Now));
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    public void MarcarTodasComoLidas()
    {
        for (var i = 0; i < Notificacoes.Count; i++)
            Notificacoes[i] = Notificacoes[i] with { Lida = true };

        Alteradas?.Invoke(this, EventArgs.Empty);
    }
}
