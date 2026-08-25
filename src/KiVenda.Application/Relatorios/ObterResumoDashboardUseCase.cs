using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Relatorios;

public sealed record ResumoDashboardDto(
    decimal VendasDeHoje,
    decimal? CaixaAtual,
    decimal LucroEstimadoHoje,
    int ProdutosStockBaixoOuSemStock,
    int VendasRealizadasHoje);

/// <summary>
/// Resumo do Dashboard (Secção 4.1: "responder de forma imediata à
/// pergunta mais comum do comerciante — Quanto vendi hoje?"). Ao
/// contrário de <see cref="GerarRelatorioDiarioUseCase"/> (módulo
/// Relatórios, restrito ao Gerente), este resumo usa apenas a
/// permissão-base (<see cref="Acao.ConsultarProdutosStockClientes"/>,
/// disponível a ambos os perfis) — é um resumo operacional do dia a
/// dia, não um relatório de gestão.
/// </summary>
public sealed class ObterResumoDashboardUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<ResumoDashboardDto> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var inicioDoDia = DateTime.UtcNow.Date;
        var fimDoDia = inicioDoDia.AddDays(1).AddTicks(-1);

        var vendasHoje = (await uow.Vendas.ListarAsync(de: inicioDoDia, ate: fimDoDia, cancellationToken: cancellationToken))
            .Where(v => v.Estado == EstadoVenda.Finalizada)
            .ToList();

        var sessaoAberta = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken);

        var produtos = await uow.Produtos.ListarAsync(apenasAtivos: true, cancellationToken: cancellationToken);
        var produtosComAlerta = produtos.Count(p => p.ObterEstadoStock() is EstadoStock.StockBaixo or EstadoStock.SemStock);

        return new ResumoDashboardDto(
            VendasDeHoje: vendasHoje.Sum(v => v.Total),
            CaixaAtual: sessaoAberta?.SaldoCalculado,
            LucroEstimadoHoje: vendasHoje.Sum(v => v.LucroEstimado),
            ProdutosStockBaixoOuSemStock: produtosComAlerta,
            VendasRealizadasHoje: vendasHoje.Count);
    }
}
