using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Relatorios;

public sealed record GerarRelatorioDiarioQuery(DateOnly Data, Guid? UtilizadorId = null);

public sealed record ProdutoVendidoDto(Guid ProdutoId, string ProdutoNome, decimal QuantidadeUnidadeBase, decimal ValorTotalVendido);

public sealed record RelatorioDiarioDto(
    DateOnly Data,
    decimal TotalVendido,
    decimal LucroEstimado,
    int NumeroDeVendas,
    IReadOnlyList<ProdutoVendidoDto> ProdutosVendidos);

/// <summary>
/// Relatório essencial do dia — total vendido, lucro, produtos vendidos
/// (Secção 4.8 da documentação funcional). Restrito ao Gerente.
/// </summary>
public sealed class GerarRelatorioDiarioUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    public async Task<RelatorioDiarioDto> ExecutarAsync(GerarRelatorioDiarioQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AcederRelatorios);

        var inicio = query.Data.ToDateTime(TimeOnly.MinValue);
        var fim = query.Data.ToDateTime(TimeOnly.MaxValue);

        var vendas = (await uow.Vendas.ListarAsync(query.UtilizadorId, de: inicio, ate: fim, cancellationToken: cancellationToken))
            .Where(v => v.Estado == EstadoVenda.Finalizada)
            .ToList();

        var totalVendido = vendas.Sum(v => v.Total);
        var lucroEstimado = vendas.Sum(v => v.LucroEstimado);

        var produtosAgregados = vendas
            .SelectMany(v => v.Itens)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(i => i.QuantidadeUnidadeBase), Valor = g.Sum(i => i.ValorTotal) })
            .ToList();

        var produtosVendidos = new List<ProdutoVendidoDto>();
        foreach (var agregado in produtosAgregados)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(agregado.ProdutoId, cancellationToken);
            produtosVendidos.Add(new ProdutoVendidoDto(agregado.ProdutoId, produto?.Nome ?? "(produto removido)", agregado.Quantidade, agregado.Valor));
        }

        return new RelatorioDiarioDto(
            query.Data,
            totalVendido,
            lucroEstimado,
            vendas.Count,
            produtosVendidos.OrderByDescending(p => p.ValorTotalVendido).ToList());
    }
}
