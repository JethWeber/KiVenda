using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Produtos;
using KiVenda.Application.Vendas;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Infrastructure.Scanner;
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
    private readonly IServicoScanner _servicoScanner;

    private ProdutoLocalizadoDto? _leituraScannerPendente;

    private Guid? _vendaId;
    private decimal _totalAtual;
    private List<ProdutoDto> _catalogo = new();
    private Guid? _categoriaSelecionada;

    [ObservableProperty]
    private bool _semCaixaAberto;

    [ObservableProperty]
    private string _vendaNumero = "—";

    [ObservableProperty]
    private bool _aCarregar;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string _termoPesquisa = string.Empty;

    [ObservableProperty]
    private string _mensagemSucesso = string.Empty;

    // ── Aviso (toast) de sucesso/erro: aparece 3 s e some sozinho ──
    private CancellationTokenSource? _toastCts;

    [ObservableProperty]
    private bool _toastVisivel;

    [ObservableProperty]
    private bool _toastErro;

    [ObservableProperty]
    private string _toastTitulo = string.Empty;

    [ObservableProperty]
    private string _toastMensagem = string.Empty;

    [ObservableProperty]
    private string _quantidadeScannerInput = "1";

    [ObservableProperty]
    private bool _mostrarQuantidadeScanner;

    [ObservableProperty]
    private bool _processandoLeituraScanner;

    [ObservableProperty]
    private string _leituraScannerPendenteTexto = string.Empty;

    public ObservableCollection<ProdutoDto> ProdutosFiltrados { get; } = new();

    /// <summary>Chips de categoria (o primeiro é sempre "Todos").</summary>
    public ObservableCollection<CategoriaFiltroItem> Categorias { get; } = new();

    /// <summary>Linhas do carrinho: itens iguais ficam agrupados numa só linha.</summary>
    public ObservableCollection<ItemCarrinho> Carrinho { get; } = new();

    /// <summary>O carrinho só aparece quando já há pelo menos um item.</summary>
    public bool CarrinhoVisivel => Carrinho.Count > 0;

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

    public event Func<ReciboVendaDto, Task<bool>>? SolicitarImpressaoRecibo;

    public VendasViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        Carrinho.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CarrinhoVisivel));
        _servicoScanner = App.Services.GetRequiredService<IServicoScanner>();
        _ = InicializarAsync();
    }

    public async Task ProcessarLeituraScannerAsync(string codigo, CancellationToken cancellationToken = default)
    {
        ProcessandoLeituraScanner = true;

        try
        {
            if (string.IsNullOrWhiteSpace(codigo) || _vendaId is null || SemCaixaAberto)
            {
                return;
            }

            MensagemErro = null;
            MensagemSucesso = string.Empty;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<LocalizarProdutoPorCodigoUseCase>();
            var localizado = await useCase.ExecutarAsync(
                new LocalizarProdutoPorCodigoQuery(codigo),
                cancellationToken);

            if (localizado is null)
            {
                TermoPesquisa = string.Empty;
                MensagemErro = $"Código \"{codigo}\" não encontrado.";
                return;
            }

            TermoPesquisa = string.Empty;

            var configuracao = _servicoScanner.ConfiguracaoAtual;

            if (configuracao.EmitirSomAoLer)
            {
                Console.Write('\a');
            }

            if (configuracao.AdicionarAutomaticamente)
            {
                await AdicionarLocalizadoAsync(localizado, 1m, cancellationToken);
                MensagemSucesso = $"✓ {localizado.Produto.Nome} — {localizado.NomeApresentacao} adicionado.";
                return;
            }

            _leituraScannerPendente = localizado;
            QuantidadeScannerInput = "1";
            LeituraScannerPendenteTexto =
                $"{localizado.Produto.Nome} — {localizado.NomeApresentacao}";

            MostrarQuantidadeScanner = true;

            if (!configuracao.AbrirQuantidadeAposLeitura)
            {
                MensagemSucesso = $"✓ {localizado.Produto.Nome} — leitura reconhecida. Defina a quantidade.";
            }
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            ProcessandoLeituraScanner = false;
        }
    }

    public async Task TratarEnterPesquisaAsync()
    {
        // Dá oportunidade ao evento CodigoLido de marcar a leitura como
        // scanner antes de decidirmos se este Enter é uma pesquisa manual.
        await Task.Yield();

        if (ProcessandoLeituraScanner)
        {
            return;
        }

        await AdicionarProdutoAsync(null);
    }

    [RelayCommand]
    private async Task ConfirmarQuantidadeScannerAsync()
    {
        if (_leituraScannerPendente is null)
        {
            return;
        }

        if (!decimal.TryParse(QuantidadeScannerInput, out var quantidade) || quantidade <= 0)
        {
            MensagemErro = "Quantidade inválida.";
            return;
        }

        try
        {
            await AdicionarLocalizadoAsync(_leituraScannerPendente, quantidade);
            MensagemSucesso =
                $"✓ {_leituraScannerPendente.Produto.Nome} — {quantidade} × {_leituraScannerPendente.NomeApresentacao} adicionado.";
            LimparLeituraScannerPendente();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
    }

    private async Task AdicionarLocalizadoAsync(
        ProdutoLocalizadoDto localizado,
        decimal quantidade,
        CancellationToken cancellationToken = default)
    {
        if (_vendaId is null)
        {
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var useCase = scope.ServiceProvider.GetRequiredService<AdicionarItemVendaUseCase>();

        await useCase.ExecutarAsync(
            new AdicionarItemVendaCommand(
                _vendaId.Value,
                localizado.Produto.Id,
                localizado.ApresentacaoId,
                quantidade),
            cancellationToken);

        await AtualizarCarrinhoAsync(scope.ServiceProvider);
    }

    private void LimparLeituraScannerPendente()
    {
        _leituraScannerPendente = null;
        QuantidadeScannerInput = "1";
        LeituraScannerPendenteTexto = string.Empty;
        MostrarQuantidadeScanner = false;
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
            var categorias = await scope.ServiceProvider.GetRequiredService<ListarCategoriasUseCase>()
                .ExecutarAsync();
            ReconstruirCategorias(categorias);
            AtualizarProdutosFiltrados();

            var vendaId = await scope.ServiceProvider.GetRequiredService<IniciarVendaUseCase>()
                .ExecutarAsync(new IniciarVendaCommand());

            _vendaId = vendaId;
            VendaNumero = FormatarNumeroVenda(vendaId);
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

    // Qualquer MensagemErro/MensagemSucesso definida no ViewModel vira aviso visual.
    // Depois de mostrada, a propriedade volta a vazio para que a mesma mensagem
    // repetida volte a disparar o aviso.
    partial void OnMensagemErroChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        MostrarToast(erro: true, value);
        MensagemErro = null;
    }

    partial void OnMensagemSucessoChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        MostrarToast(erro: false, value);
        MensagemSucesso = string.Empty;
    }

    private async void MostrarToast(bool erro, string mensagem)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();

        ToastErro = erro;
        ToastTitulo = erro ? "Erro" : "Sucesso";
        ToastMensagem = mensagem.TrimStart('✓', ' ');
        ToastVisivel = true;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            ToastVisivel = false;
        }
        catch (OperationCanceledException)
        {
            // Chegou outro aviso: ele reinicia a contagem.
        }
    }

    private void AtualizarProdutosFiltrados()
    {
        ProdutosFiltrados.Clear();

        IEnumerable<ProdutoDto> query = _catalogo;

        if (_categoriaSelecionada is not null)
        {
            query = query.Where(p => p.CategoriaId == _categoriaSelecionada);
        }

        if (!string.IsNullOrWhiteSpace(TermoPesquisa))
        {
            query = query.Where(p =>
                p.Nome.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                p.CodigoInterno.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase) ||
                (p.CodigoBarras is not null && p.CodigoBarras.Contains(TermoPesquisa, StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var produto in query)
        {
            ProdutosFiltrados.Add(produto);
        }
    }

    private void ReconstruirCategorias(IReadOnlyList<CategoriaDto> categorias)
    {
        Categorias.Clear();
        Categorias.Add(new CategoriaFiltroItem("Todos", null) { Selecionada = true });
        _categoriaSelecionada = null;

        // Só mostra categorias que têm pelo menos um produto no catálogo.
        var usadas = _catalogo.Select(p => p.CategoriaId).ToHashSet();

        foreach (var categoria in categorias.Where(c => usadas.Contains(c.Id)).OrderBy(c => c.Nome))
        {
            Categorias.Add(new CategoriaFiltroItem(categoria.Nome, categoria.Id));
        }
    }

    [RelayCommand]
    private void SelecionarCategoria(CategoriaFiltroItem categoria)
    {
        foreach (var chip in Categorias)
        {
            chip.Selecionada = chip == categoria;
        }

        _categoriaSelecionada = categoria.Categoria;
        AtualizarProdutosFiltrados();
    }

    // Mesmo formato do número do recibo (8 primeiros caracteres do Id).
    private static string FormatarNumeroVenda(Guid vendaId)
        => vendaId.ToString()[..8].ToUpperInvariant();

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
    private async Task RemoverItemAsync(ItemCarrinho linha)
    {
        if (_vendaId is null)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RemoverItemVendaUseCase>();

            foreach (var item in linha.Itens)
            {
                await useCase.ExecutarAsync(new RemoverItemVendaCommand(_vendaId.Value, item.Id));
            }

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

            var imprimir = SolicitarImpressaoRecibo is not null
                && await SolicitarImpressaoRecibo.Invoke(recibo);

            MensagemSucesso = imprimir
                ? $"Venda concluída — recibo {recibo.VendaId.ToString()[..8].ToUpperInvariant()} impresso."
                : $"Venda concluída — recibo {recibo.VendaId.ToString()[..8].ToUpperInvariant()}.";

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

    /// <summary>
    /// Botão "Nova venda". Ainda não existe "suspender venda" no domínio, por isso
    /// descarta o carrinho atual (se tiver itens) e abre uma venda limpa.
    /// </summary>
    [RelayCommand]
    private async Task NovaVendaManualAsync()
    {
        if (Carrinho.Count == 0)
        {
            return;
        }

        await CancelarVendaAsync();
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
            VendaNumero = FormatarNumeroVenda(_vendaId.Value);
            SemCaixaAberto = false;
        }
        catch (DomainException ex)
        {
            _vendaId = null;
            VendaNumero = "—";
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
        foreach (var grupo in venda.Itens.GroupBy(i => (i.ProdutoNome, i.ApresentacaoNome)))
        {
            Carrinho.Add(new ItemCarrinho(grupo.ToList()));
        }

        _totalAtual = venda.Total;
        SubtotalTexto = FormatadorKz.Formatar(venda.Subtotal);
        DescontoTexto = FormatadorKz.Formatar(venda.Desconto);
        TotalTexto = FormatadorKz.Formatar(venda.Total);
    }
}

/// <summary>Chip de categoria da grelha de produtos do PDV.</summary>
public partial class CategoriaFiltroItem : ObservableObject
{
    public CategoriaFiltroItem(string nome, Guid? categoria)
    {
        Nome = nome;
        Categoria = categoria;
    }

    /// <summary>Texto mostrado no chip.</summary>
    public string Nome { get; }

    /// <summary>Id da categoria usado no filtro; null = "Todos".</summary>
    public Guid? Categoria { get; }

    [ObservableProperty]
    private bool _selecionada;
}

/// <summary>
/// Uma linha do carrinho. Se o mesmo produto/apresentação foi adicionado várias
/// vezes, os itens ficam agrupados aqui e a quantidade e o total são somados.
/// </summary>
public sealed class ItemCarrinho
{
    public ItemCarrinho(IReadOnlyList<ItemVendaDto> itens)
    {
        Itens = itens;
    }

    /// <summary>Itens reais da venda que compõem esta linha.</summary>
    public IReadOnlyList<ItemVendaDto> Itens { get; }

    public string ProdutoNome => Itens[0].ProdutoNome;

    public string ApresentacaoNome => Itens[0].ApresentacaoNome;

    public decimal Quantidade => Itens.Sum(i => (decimal)i.QuantidadeNaApresentacao);

    public decimal ValorTotal => Itens.Sum(i => (decimal)i.ValorTotal);
}
