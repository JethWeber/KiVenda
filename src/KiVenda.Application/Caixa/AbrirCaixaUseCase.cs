using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Caixa;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Caixa;

public sealed record AbrirCaixaCommand(decimal SaldoInicial);

/// <summary>
/// O MVP assume um único caixa aberto de cada vez (ver Fase 6/7 do
/// plano de implementação) — por isso valida aqui que não existe já
/// nenhuma sessão aberta antes de criar uma nova.
/// </summary>
public sealed class AbrirCaixaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(AbrirCaixaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessaoJaAberta = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken);
        if (sessaoJaAberta is not null)
        {
            throw new DomainException("Já existe uma sessão de caixa aberta. Feche-a antes de abrir uma nova.");
        }

        var sessao = new SessaoCaixa(contexto.UtilizadorId, comando.SaldoInicial);

        await uow.SessoesCaixa.AdicionarAsync(sessao, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return sessao.Id;
    }
}
