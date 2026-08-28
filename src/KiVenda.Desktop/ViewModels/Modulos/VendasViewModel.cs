using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Produtos;
using KiVenda.Application.Vendas;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Infrastructure.Impressao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Módulo central do sistema (Secção 4.4). Fluxo: Selecionar produto →
/// (apresentação padrão, ver nota abaixo) → Receber pagamento → Emitir
/// recibo → stock/caixa atualizados pelo próprio FinalizarVendaUseCase.
///
/// Simplificação desta fase: ao clicar num produto com várias
/// apresentações comerciais, usa-se sempre a primeira apresentação
/// ativa (mesma simplificação já aceite em Compras, Fase 6) — escolher
/// entre apresentações no ato da venda fica para um refinamento futuro
/// deste ecrã.
/// </summary>
public partial class VendasViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    private Guid? _vendaId;
    private decimal _totalAtual;
    private List<ProdutoDto> _catalogo = new();

    [ObservableProperty]
    private bool _semCaixaAberto;

    [ObservableProperty]
    private bool _aCarregar;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string _termoPesquisa = string.Empty;

    [ObservableProperty]
    private string _mensagemSucesso = string.Empty;

    public ObservableCollection<ProdutoDto> ProdutosFiltrados { get; } = new();

    public ObservableCollection<ItemVendaDto> Carrinho { get; } = new();

    [ObservableProperty]
    private string _subtotalTexto = "0 Kz";

    [ObservableProperty]
    private string _descontoTexto = "0 Kz";

    [ObservableProperty]
    private string _totalTexto = "0 Kz";

    [ObservableProperty]
    private string _descontoInput = string.Empty;

    [ObservableProperty]
    private MetodoPagamento _metodoSelecionado = MetodoPagamento.Dinheiro;

    [ObservableProperty]
    private string _valorPago = string.Empty;

    [ObservableProperty]
    private bool _aFinalizar;

    public IReadOnlyList<MetodoPagamento> MetodosPagamento { get; } =
        new[] { MetodoPagamento.Dinheiro, MetodoPagamento.Multicaixa, MetodoPagamento.Tpa };

    public VendasViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        ACarregar = true;
        MensagemErro = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var produtos = await scope.ServiceProvider.GetRequiredService<ListarProdutosUseCase>()
                .ExecutarAsync(new ListarProdutosQuery());
            _catalogo = produtos.ToList();
            AtualizarProdutosFiltrados();

            var vendaId = await scope.ServiceProvider.GetRequiredService<IniciarVendaUseCase>()
                .ExecutarAsync(new IniciarVendaCommand());

            _vendaId = vendaId;
            SemCaixaAberto = false;
        }
        catch (DomainException ex)
        {
            // Tipicamente "Não há nenhuma sessão de caixa aberta." — ver IniciarVendaUseCase (Fase 3).
            SemCaixaAberto = true;
            MensagemErro = ex.Message;
        }
        finally
        {
            ACarregar = false;
        }
    }

    partial void OnTermoPesquisaChanged(string value) => AtualizarProdutosFiltrados();

    private void AtualizarProdutosFiltrados()
    {
        ProdutosFiltrados.Clear();

        var query = string.IsNullOrWhiteSpace(TermoPesquisa)
            ? _catalogo
            : _catalogo.Where(p =>
                p.Nome.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                p.CodigoInterno.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                (p.CodigoBarras is not null && p.CodigoBarras.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase)));

        foreach (var produto in query)
        {
            ProdutosFiltrados.Add(produto);
        }
    }

    /// <summary>
    /// Chamado tanto ao clicar num produto na grelha como ao premir
    /// Enter na pesquisa com um código exato (o mesmo comportamento que
    /// o scanner de código de barras vai desencadear na Fase 8 — este
    /// campo já funciona como a base desse fluxo).
    /// </summary>
    [RelayCommand]
    private async Task AdicionarProdutoAsync(ProdutoDto? produto)
    {
        produto ??= _catalogo.FirstOrDefault(p =>
            p.CodigoBarras == TermoPesquisa || p.CodigoInterno == TermoPesquisa);

        if (produto is null || _vendaId is null)
        {
            return;
        }

        var apresentacao = produto.Apresentacoes.FirstOrDefault(a => a.Ativa);
        if (apresentacao is null)
        {
            MensagemErro = $"\"{produto.Nome}\" não tem nenhuma apresentação ativa.";
            return;
        }

        MensagemErro = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<AdicionarItemVendaUseCase>();

            await useCase.ExecutarAsync(new AdicionarItemVendaCommand(_vendaId.Value, produto.Id, apresentacao.Id, 1));

            TermoPesquisa = string.Empty;
            await AtualizarCarrinhoAsync(scope.ServiceProvider);
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RemoverItemAsync(ItemVendaDto item)
    {
        if (_vendaId is null)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RemoverItemVendaUseCase>();

            await useCase.ExecutarAsync(new RemoverItemVendaCommand(_vendaId.Value, item.Id));

            await AtualizarCarrinhoAsync(scope.ServiceProvider);
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
    }

    [RelayCommand]
    private async Task AplicarDescontoAsync()
    {
        if (_vendaId is null)
        {
            return;
        }

        if (!decimal.TryParse(DescontoInput, out var desconto) || desconto < 0)
        {
            MensagemErro = "Desconto inválido.";
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<AplicarDescontoVendaUseCase>();

            await useCase.ExecutarAsync(new AplicarDescontoVendaCommand(_vendaId.Value, desconto));

            await AtualizarCarrinhoAsync(scope.ServiceProvider);
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
    }

    [RelayCommand]
    private async Task FinalizarVendaAsync()
    {
        if (_vendaId is null || Carrinho.Count == 0)
        {
            MensagemErro = "Adicione pelo menos um item antes de receber o pagamento.";
            return;
        }

        decimal valorPago;
        if (string.IsNullOrWhiteSpace(ValorPago))
        {
            valorPago = _totalAtual;
        }
        else if (!decimal.TryParse(ValorPago, out valorPago) || valorPago < 0)
        {
            MensagemErro = "Valor pago inválido.";
            return;
        }

        AFinalizar = true;
        MensagemErro = null;
        MensagemSucesso = string.Empty;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<FinalizarVendaUseCase>();

            var recibo = await useCase.ExecutarAsync(new FinalizarVendaCommand(
                _vendaId.Value,
                new[] { new PagamentoCommand(MetodoSelecionado, valorPago) }));

            var servicoImpressao = scope.ServiceProvider.GetRequiredService<IServicoImpressao>();
            // Dados da loja fixos por agora — o ecrã de edição (Fase 11,
            // Configurações → Dados da Loja) vai substituir isto por dados reais.
            await servicoImpressao.ImprimirReciboVendaAsync(recibo, new DadosLoja("KiVenda"));

            MensagemSucesso = $"Venda concluída — recibo {recibo.VendaId.ToString()[..8].ToUpperInvariant()}.";

            await NovaVendaAsync();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AFinalizar = false;
        }
    }

    [RelayCommand]
    private async Task CancelarVendaAsync()
    {
        if (_vendaId is null)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<CancelarVendaUseCase>();
            await useCase.ExecutarAsync(new CancelarVendaCommand(_vendaId.Value));
        }
        catch (DomainException)
        {
            // Se já não estava em andamento, não há nada a fazer.
        }

        await NovaVendaAsync();
    }

    private async Task NovaVendaAsync()
    {
        Carrinho.Clear();
        DescontoInput = string.Empty;
        ValorPago = string.Empty;
        _totalAtual = 0;
        SubtotalTexto = FormatadorKz.Formatar(0);
        DescontoTexto = FormatadorKz.Formatar(0);
        TotalTexto = FormatadorKz.Formatar(0);

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<IniciarVendaUseCase>();
            _vendaId = await useCase.ExecutarAsync(new IniciarVendaCommand());
            SemCaixaAberto = false;
        }
        catch (DomainException ex)
        {
            _vendaId = null;
            SemCaixaAberto = true;
            MensagemErro = ex.Message;
        }
    }

    private async Task AtualizarCarrinhoAsync(IServiceProvider servicos)
    {
        if (_vendaId is null)
        {
            return;
        }

        var useCase = servicos.GetRequiredService<ConsultarVendaUseCase>();
        var venda = await useCase.ExecutarAsync(new ConsultarVendaQuery(_vendaId.Value));

        Carrinho.Clear();
        foreach (var item in venda.Itens)
        {
            Carrinho.Add(item);
        }

        _totalAtual = venda.Total;
        SubtotalTexto = FormatadorKz.Formatar(venda.Subtotal);
        DescontoTexto = FormatadorKz.Formatar(venda.Desconto);
        TotalTexto = FormatadorKz.Formatar(venda.Total);
    }
}
