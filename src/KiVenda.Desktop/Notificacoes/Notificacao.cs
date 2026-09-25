namespace KiVenda.Desktop.Notificacoes;

public sealed record Notificacao(
    string Titulo,
    string Mensagem,
    DateTime Data,
    bool Lida = false)
{
    public string DataFormatada => Data.ToString("dd/MM/yyyy HH:mm");
}
