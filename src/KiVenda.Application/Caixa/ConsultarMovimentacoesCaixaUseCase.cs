using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Caixa;

public sealed record ConsultarMovimentacoesCaixaQuery(Guid? SessaoCaixaId = null);

public sealed record MovimentoCaixaDto(Guid Id, TipoMovimentoCaixa Tipo, decimal Valor, Guid UtilizadorId, string? Descricao, Guid? OrigemVendaId, DateTime Data);

public sealed record ResumoCaixaDto(
    Guid SessaoCaixaId,
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoCalculado,
    IReadOnlyList<MovimentoCaixaDto> Movimentos);

/// <summary>
/// Corresponde ao ecrã "Resumo Rápido" + "Últimas Movimentações" do
/// módulo Caixa (Fase 7.2). Sem <see cref="ConsultarMovimentacoesCaixaQuery.SessaoCaixaId"/>,
/// consulta a sessão atualmente aberta.
/// </summary>
public sealed class ConsultarMovimentacoesCaixaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<ResumoCaixaDto> ExecutarAsync(ConsultarMovimentacoesCaixaQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessao = query.SessaoCaixaId.HasValue
            ? await uow.SessoesCaixa.ObterPorIdAsync(query.SessaoCaixaId.Value, cancellationToken)
            : await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken);

        if (sessao is null)
        {
            throw new DomainException("Sessão de caixa não encontrada.");
        }

        var movimentos = sessao.Movimentos
            .OrderByDescending(m => m.Data)
            .Select(m => new MovimentoCaixaDto(m.Id, m.Tipo, m.Valor, m.UtilizadorId, m.Descricao, m.OrigemVendaId, m.Data))
            .ToList();

        return new ResumoCaixaDto(sessao.Id, sessao.SaldoInicial, sessao.TotalEntradas, sessao.TotalSaidas, sessao.SaldoCalculado, movimentos);
    }
}
