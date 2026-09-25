using KiVenda.Core.Common;
using KiVenda.Core.Exceptions;

namespace KiVenda.Core.Notificacoes;

public sealed class Notificacao : Entity
{
    public Guid UtilizadorId { get; private set; }
    public string Tipo { get; private set; } = null!;
    public string Titulo { get; private set; } = null!;
    public string Mensagem { get; private set; } = null!;
    public DateTime DataCriacao { get; private set; }
    public bool Lida { get; private set; }
    public DateTime? DataLeitura { get; private set; }

    private Notificacao() { }

    public Notificacao(Guid utilizadorId, string tipo, string titulo, string mensagem)
    {
        if (utilizadorId == Guid.Empty) throw new DomainException("O utilizador da notificação é obrigatório.");
        if (string.IsNullOrWhiteSpace(tipo)) throw new DomainException("O tipo da notificação é obrigatório.");
        if (string.IsNullOrWhiteSpace(titulo)) throw new DomainException("O título da notificação é obrigatório.");
        if (string.IsNullOrWhiteSpace(mensagem)) throw new DomainException("A mensagem da notificação é obrigatória.");

        UtilizadorId = utilizadorId;
        Tipo = tipo.Trim();
        Titulo = titulo.Trim();
        Mensagem = mensagem.Trim();
        DataCriacao = DateTime.UtcNow;
    }

    public void MarcarComoLida()
    {
        if (Lida) return;
        Lida = true;
        DataLeitura = DateTime.UtcNow;
        MarcarComoAtualizado();
    }
}
