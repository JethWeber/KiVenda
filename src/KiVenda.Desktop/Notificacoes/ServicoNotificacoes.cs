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
    private CancellationTokenSource? _atualizacaoCts;
    private readonly HashSet<Guid> _produtosEmStockAlerta = new();

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
        var produtosComStockBaixo = produtos
            .Where(p => p.ObterEstadoStock() is KiVenda.Core.Enums.EstadoStock.StockBaixo or KiVenda.Core.Enums.EstadoStock.SemStock)
            .ToList();

        var idsEmAlertaAgora = produtosComStockBaixo.Select(p => p.Id).ToHashSet();
        _produtosEmStockAlerta.RemoveWhere(id => !idsEmAlertaAgora.Contains(id));

        foreach (var produto in produtosComStockBaixo)
        {
            // Só gera o alerta na transição normal -> stock baixo.
            // Se o utilizador eliminar a notificação, ela não volta a aparecer
            // enquanto o produto continuar no mesmo estado.
            if (!_produtosEmStockAlerta.Add(produto.Id))
                continue;

            var tipo = $"STOCK_BAIXO:{produto.Id:N}";
            var titulo = produto.EstoqueAtual <= 0 ? "Produto sem stock" : "Stock baixo";
            var mensagem = produto.EstoqueAtual <= 0
                ? $"O produto {produto.Nome} ficou sem stock."
                : $"O stock de {produto.Nome} está baixo: {produto.EstoqueAtual:0.####} unidade(s).";

            var entidade = new KiVenda.Core.Notificacoes.Notificacao(_sessao.UtilizadorId, tipo, titulo, mensagem);
            await uow.Notificacoes.AdicionarAsync(entidade, cancellationToken);
            Notificacoes.Insert(0, Mapear(entidade));
        }

        if (produtosComStockBaixo.Count > 0)
            await uow.SaveChangesAsync(cancellationToken);
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_sessao.UtilizadorId == Guid.Empty) return;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        await uow.Notificacoes.RemoverAsync(id, _sessao.UtilizadorId, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        var item = Notificacoes.FirstOrDefault(x => x.Id == id);
        if (item is not null)
            Notificacoes.Remove(item);
        Alteradas?.Invoke(this, EventArgs.Empty);
    }

    public void IniciarAtualizacaoAutomatica()
    {
        if (_atualizacaoCts is not null) return;
        _atualizacaoCts = new CancellationTokenSource();
        _ = AtualizarPeriodicamenteAsync(_atualizacaoCts.Token);
    }

    public void PararAtualizacaoAutomatica()
    {
        _atualizacaoCts?.Cancel();
        _atualizacaoCts?.Dispose();
        _atualizacaoCts = null;
    }

    private async Task AtualizarPeriodicamenteAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                if (!cancellationToken.IsCancellationRequested)
                    await CarregarAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
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