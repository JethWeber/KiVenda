using System.Collections.ObjectModel;
using KiVenda.Application.Abstractions.Persistence;
using KiVenda.Core.Notificacoes;
using KiVenda.Desktop.Autenticacao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.Notificacoes;

public sealed class ServicoNotificacoes
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;
    public ObservableCollection<Notificacao> Notificacoes { get; } = new();
    public event EventHandler? Alteradas;
    public int NaoLidas => Notificacoes.Count(x => !x.Lida);

    public ServicoNotificacoes(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
    }

    public async Task CarregarAsync(CancellationToken cancellationToken = default)
    {
        Notificacoes.Clear();
        if (_sessao.UtilizadorId == Guid.Empty) { Alteradas?.Invoke(this, EventArgs.Empty); return; }
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUnitOfWork>().Notificacoes;
        var itens = await repo.ListarPorUtilizadorAsync(_sessao.UtilizadorId, cancellationToken: cancellationToken);
        foreach (var item in itens)
            Notificacoes.Add(Mapear(item));

        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var produtos = await uow.Produtos.ListarAsync(apenasAtivos: true, cancellationToken: cancellationToken);
        var ultimaPorTipo = itens.GroupBy(x => x.Tipo).ToDictionary(g => g.Key, g => g.Max(x => x.DataCriacao));

        foreach (var produto in produtos.Where(p => p.ObterEstadoStock() is KiVenda.Core.Enums.EstadoStock.StockBaixo or KiVenda.Core.Enums.EstadoStock.SemStock))
        {
            var tipo = $"STOCK_BAIXO:{produto.Id:N}";
            if (ultimaPorTipo.TryGetValue(tipo, out var ultima) && DateTime.UtcNow - ultima < TimeSpan.FromDays(3))
                continue;

            var titulo = produto.EstoqueAtual <= 0 ? "Produto sem stock" : "Stock baixo";
            var mensagem = produto.EstoqueAtual <= 0
                ? $"O produto {produto.Nome} ficou sem stock."
                : $"O stock de {produto.Nome} está baixo: {produto.EstoqueAtual:0.####} unidade(s).";

            var entidade = new KiVenda.Core.Notificacoes.Notificacao(_sessao.UtilizadorId, tipo, titulo, mensagem);
            await uow.Notificacoes.AdicionarAsync(entidade, cancellationToken);
            Notificacoes.Insert(0, Mapear(entidade));
            ultimaPorTipo[tipo] = entidade.DataCriacao;
        }

        if (produtos.Any(p => p.ObterEstadoStock() is KiVenda.Core.Enums.EstadoStock.StockBaixo or KiVenda.Core.Enums.EstadoStock.SemStock))
            await uow.SaveChangesAsync(cancellationToken);
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    public async Task AdicionarAsync(string tipo, string titulo, string mensagem, CancellationToken cancellationToken = default)
    {
        if (_sessao.UtilizadorId == Guid.Empty) return;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entidade = new KiVenda.Core.Notificacoes.Notificacao(_sessao.UtilizadorId, tipo, titulo, mensagem);
        await uow.Notificacoes.AdicionarAsync(entidade, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        Notificacoes.Insert(0, Mapear(entidade));
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    public async Task MarcarTodasComoLidasAsync(CancellationToken cancellationToken = default)
    {
        if (_sessao.UtilizadorId == Guid.Empty) return;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var itens = await uow.Notificacoes.ListarPorUtilizadorAsync(_sessao.UtilizadorId, cancellationToken: cancellationToken);
        foreach (var item in itens) item.MarcarComoLida();
        await uow.SaveChangesAsync(cancellationToken);
        for (var i = 0; i < Notificacoes.Count; i++)
            Notificacoes[i] = Notificacoes[i] with { Lida = true };
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    private static Notificacao Mapear(KiVenda.Core.Notificacoes.Notificacao n) =>
        new(n.Id, n.Tipo, n.Titulo, n.Mensagem, n.DataCriacao, n.Lida);
}