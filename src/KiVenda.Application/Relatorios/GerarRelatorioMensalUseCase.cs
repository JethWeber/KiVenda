using KiVenda.Application.Abstractions.Auth;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Application.Common;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;

namespace KiVenda.Application.Relatorios;

public sealed record GerarRelatorioMensalQuery(int Ano, int Mes);

public sealed record RelatorioMensalDto(
    int Ano,
    int Mes,
    decimal Receita,
    decimal LucroEstimado,
    IReadOnlyList<ProdutoVendidoDto> ProdutosMaisVendidos);

/// <summary>
/// Relatório essencial mensal — receita, lucro, produtos mais vendidos
/// (Secção 4.8). Restrito ao Gerente.
/// </summary>
public sealed class GerarRelatorioMensalUseCase(IUnitOfWork uow, IContextoAutenticacao contexto)
{
    private const int LimiteProdutosMaisVendidos = 10;

    public async Task<RelatorioMensalDto> ExecutarAsync(GerarRelatorioMensalQuery query, CancellationToken cancellationToken = default)
    {
        PermissaoGuard.Exigir(contexto, Acao.AcederRelatorios);

        var inicio = new DateTime(query.Ano, query.Mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fim = inicio.AddMonths(1).AddTicks(-1);

        var vendas = (await uow.Vendas.ListarAsync(de: inicio, ate: fim, cancellationToken: cancellationToken))
            .Where(v => v.Estado == EstadoVenda.Finalizada)
            .ToList();

        var receita = vendas.Sum(v => v.Total);
        var lucroEstimado = vendas.Sum(v => v.LucroEstimado);

        var produtosAgregados = vendas
            .SelectMany(v => v.Itens)
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(i => i.QuantidadeUnidadeBase), Valor = g.Sum(i => i.ValorTotal) })
            .OrderByDescending(g => g.Quantidade)
            .Take(LimiteProdutosMaisVendidos)
            .ToList();

        var produtosMaisVendidos = new List<ProdutoVendidoDto>();
        foreach (var agregado in produtosAgregados)
        {
            var produto = await uow.Produtos.ObterPorIdAsync(agregado.ProdutoId, cancellationToken);
            produtosMaisVendidos.Add(new ProdutoVendidoDto(agregado.ProdutoId, produto?.Nome ?? "(produto removido)", agregado.Quantidade, agregado.Valor));
        }

        return new RelatorioMensalDto(query.Ano, query.Mes, receita, lucroEstimado, produtosMaisVendidos);
    }
}
