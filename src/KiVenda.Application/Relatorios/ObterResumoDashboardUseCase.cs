using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Relatorios;

public sealed record AlertaDashboardDto(
    Guid Id,
    string Tipo,
    string Titulo,
    string Mensagem);

public sealed record VendaDiaDashboardDto(
    DateTime Data,
    decimal Total);

public sealed record ResumoDashboardDto(
    decimal VendasDeHoje,
    decimal? CaixaAtual,
    decimal LucroEstimadoHoje,
    int ProdutosStockBaixoOuSemStock,
    int VendasRealizadasHoje,
    IReadOnlyList<AlertaDashboardDto> Alertas,
    IReadOnlyList<VendaDiaDashboardDto> VendasPorDia);

/// <summary>
/// Resumo do Dashboard (Secção 4.1: "responder de forma imediata à
/// pergunta mais comum do comerciante — Quanto vendi hoje?"). Ao
/// contrário de <see cref="GerarRelatorioDiarioUseCase"/> (módulo
/// Relatórios, restrito ao Gerente), este resumo usa apenas a
/// permissão-base (<see cref="Acao.ConsultarProdutosStockClientes"/>),
/// disponível a ambos os perfis.
/// </summary>
public sealed class ObterResumoDashboardUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<ResumoDashboardDto> ExecutarAsync(CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.ConsultarProdutosStockClientes);

        var inicioHistoricoLocal = DateTime.Now.Date.AddDays(-29);
        var fimHistoricoLocal = DateTime.Now.Date.AddDays(1);

        var inicioHistoricoUtc = inicioHistoricoLocal.ToUniversalTime();
        var fimHistoricoUtc = fimHistoricoLocal.ToUniversalTime();

        var vendasHistorico = (await uow.Vendas.ListarAsync(
                de: inicioHistoricoUtc,
                ate: fimHistoricoUtc.AddTicks(-1),
                cancellationToken: cancellationToken))
            .Where(v => v.Estado == EstadoVenda.Finalizada)
            .ToList();

        var vendasHoje = vendasHistorico
            .Where(v => v.Data.ToLocalTime().Date == DateTime.Now.Date)
            .ToList();

        var vendasPorData = vendasHistorico
            .GroupBy(v => v.Data.ToLocalTime().Date)
            .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

        var vendasPorDia = Enumerable.Range(0, 30)
            .Select(offset =>
            {
                var data = inicioHistoricoLocal.AddDays(offset);
                return new VendaDiaDashboardDto(
                    data,
                    vendasPorData.GetValueOrDefault(data, 0m));
            })
            .ToList();

        var sessaoAberta = await uow.SessoesCaixa.ObterAbertaAsync(cancellationToken);

        var produtos = await uow.Produtos.ListarAsync(
            apenasAtivos: true,
            cancellationToken: cancellationToken);

        var produtosComAlerta = produtos
            .Where(p => p.ObterEstadoStock() is EstadoStock.StockBaixo or EstadoStock.SemStock)
            .ToList();

        var notificacoes = await uow.Notificacoes.ListarPorUtilizadorAsync(
            contexto.UtilizadorId,
            cancellationToken: cancellationToken);

        var alertas = notificacoes
            .Where(n => !n.Lida)
            .OrderByDescending(n => n.DataCriacao)
            .Select(n => new AlertaDashboardDto(
                n.Id,
                n.Tipo,
                n.Titulo,
                n.Mensagem))
            .ToList();

        return new ResumoDashboardDto(
            VendasDeHoje: vendasHoje.Sum(v => v.Total),
            CaixaAtual: sessaoAberta?.SaldoCalculado,
            LucroEstimadoHoje: vendasHoje.Sum(v => v.LucroEstimado),
            ProdutosStockBaixoOuSemStock: produtosComAlerta.Count,
            VendasRealizadasHoje: vendasHoje.Count,
            Alertas: alertas,
            VendasPorDia: vendasPorDia);
    }
}
