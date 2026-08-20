using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Exceptions;

/// <summary>
/// Lançada quando o perfil do utilizador autenticado não tem permissão
/// para a ação pedida (ver <see cref="Permissoes"/>, Fase 1). Distinta de
/// <see cref="KiVenda.Core.Exceptions.DomainException"/> porque não é uma
/// violação de regra de negócio do domínio, mas sim de controlo de
/// acesso da aplicação — a UI (Fase 6) pode querer tratar as duas de
/// forma diferente (ex.: esconder o botão vs. mostrar um erro de
/// validação).
/// </summary>
public sealed class PermissaoNegadaException : Exception
{
    public Acao Acao { get; }

    public PermissaoNegadaException(Acao acao)
        : base($"O perfil atual não tem permissão para executar a ação \"{acao}\".")
    {
        Acao = acao;
    }
}
