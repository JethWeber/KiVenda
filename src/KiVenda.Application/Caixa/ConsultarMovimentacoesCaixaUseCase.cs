using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Caixa;

public sealed record ConsultarMovimentacoesCaixaQuery(Guid? SessaoCaixaId = null);

public sealed record MovimentoCaixaDto(
    Guid Id,
    TipoMovimentoCaixa Tipo,
    decimal Valor,
    Guid UtilizadorId,
    string? Descricao,
    Guid? OrigemVendaId,
    DateTime Data,
    string? UtilizadorNome = null);

public sealed record ResumoCaixaDto(
    Guid SessaoCaixaId,
    decimal SaldoInicial,
    decimal TotalEntradas,
    decimal TotalSaidas,
    decimal SaldoCalculado,
    IReadOnlyList<MovimentoCaixaDto> Movimentos);

public sealed class ConsultarMovimentacoesCaixaUseCase(
    IUnitOfWork uow,
    IContextoAutenticacao contexto)
{
    public async Task<ResumoCaixaDto> ExecutarAsync(
        ConsultarMovimentacoesCaixaQuery query,
        CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessao = query.SessaoCaixaId.HasValue
            ? await uow.SessoesCaixa.ObterPorIdAsync(query.SessaoCaixaId.Value, cancellationToken)
            : await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken);

        if (sessao is null)
            throw new DomainException("Sessão de caixa não encontrada.");

        var movimentos = sessao.Movimentos
            .OrderByDescending(m => m.Data)
            .ToList();

        var utilizadores = new Dictionary<Guid, string>();
        foreach (var id in movimentos.Select(m => m.UtilizadorId).Distinct())
        {
            var utilizador = await uow.Utilizadores.ObterPorIdAsync(id, cancellationToken);
            if (utilizador is not null)
                utilizadores[id] = utilizador.Nome;
        }

        var dto = movimentos
            .Select(m => new MovimentoCaixaDto(
                m.Id,
                m.Tipo,
                m.Valor,
                m.UtilizadorId,
                m.Descricao,
                m.OrigemVendaId,
                m.Data,
                utilizadores.TryGetValue(m.UtilizadorId, out var nome) ? nome : "Utilizador desconhecido"))
            .ToList();

        return new ResumoCaixaDto(
            sessao.Id,
            sessao.SaldoInicial,
            sessao.TotalEntradas,
            sessao.TotalSaidas,
            sessao.SaldoCalculado,
            dto);
    }
}
