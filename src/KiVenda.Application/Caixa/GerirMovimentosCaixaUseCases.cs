using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Caixa;

public sealed record RegistarSuprimentoCommand(decimal Valor, string? Descricao = null);

public sealed record RegistarSangriaCommand(decimal Valor, string? Descricao = null);

public sealed class RegistarSuprimentoUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarSuprimentoCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessao = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken)
            ?? throw new DomainException("Não há nenhuma sessão de caixa aberta.");

        var movimento = sessao.RegistarSuprimento(comando.Valor, contexto.UtilizadorId, comando.Descricao);

        await uow.SaveChangesAsync(cancellationToken);

        return movimento.Id;
    }
}

public sealed class RegistarSangriaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<Guid> ExecutarAsync(RegistarSangriaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessao = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken)
            ?? throw new DomainException("Não há nenhuma sessão de caixa aberta.");

        var movimento = sessao.RegistarSangria(comando.Valor, contexto.UtilizadorId, comando.Descricao);

        // Sangria é o exemplo dado na própria UI (ver mockups do Caixa) —
        // não é obrigatória por auditoria, mas é boa prática registar
        // saídas manuais de dinheiro para reforçar a confiança do gerente.
        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(contexto.UtilizadorId, "Sangria de caixa", "SessaoCaixa", sessao.Id, dadosDepois: comando.Valor.ToString("0.00")),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return movimento.Id;
    }
}
