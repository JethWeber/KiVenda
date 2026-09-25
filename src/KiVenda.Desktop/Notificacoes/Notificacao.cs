namespace KiVenda.Desktop.Notificacoes;

public sealed record Notificacao(
    Guid Id,
    string Tipo,
    string Titulo,
    string Mensagem,
    DateTime Data,
    bool Lida)
{
    public string DataFormatada => Data.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}