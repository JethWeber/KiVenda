using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Auditoria;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Caixa;

public sealed record FecharCaixaCommand(decimal SaldoInformado);

public sealed record FechoCaixaDto(decimal SaldoCalculado, decimal SaldoInformado, decimal Divergencia);

/// <summary>
/// Fecha a sessão de caixa atualmente aberta e regista sempre um evento
/// de auditoria com a divergência apurada — essencial para o gerente
/// identificar discrepâncias (Secção 7 da documentação funcional).
/// </summary>
public sealed class FecharCaixaUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<FechoCaixaDto> ExecutarAsync(FecharCaixaCommand comando, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.GerirCaixa);

        var sessao = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken)
            ?? throw new DomainException("Não há nenhuma sessão de caixa aberta para fechar.");

        var saldoCalculado = sessao.SaldoCalculado;
        var divergencia = sessao.Fechar(comando.SaldoInformado, contexto.UtilizadorId);

        await uow.LogsAuditoria.AdicionarAsync(
            new LogAuditoria(
                contexto.UtilizadorId,
                "Fechou caixa",
                "SessaoCaixa",
                sessao.Id,
                dadosAntes: $"Esperado: {saldoCalculado}",
                dadosDepois: $"Informado: {comando.SaldoInformado} (divergência: {divergencia})"),
            cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);

        return new FechoCaixaDto(saldoCalculado, comando.SaldoInformado, divergencia);
    }
}
