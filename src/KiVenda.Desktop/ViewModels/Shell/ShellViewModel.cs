using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Empresas;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.Notificacoes;
using KiVenda.Desktop.Tema;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Desktop.ViewModels.Modulos;
using Microsoft.Extensions.DependencyInjection;
using WeberTech.Licensing.Enums;
using WeberTech.Licensing.Services;

namespace KiVenda.Desktop.ViewModels.Shell;

public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;
    private readonly ServicoNotificacoes _servicoNotificacoes;
    private readonly ServicoTema _servicoTema;
    private readonly DispatcherTimer _timerLicenca;

    public string NomeUtilizador => _sessao.Nome;
    public string IniciaisNome => ObterIniciais(_sessao.Nome);
    public string PerfilTexto => _sessao.Perfil == PerfilUtilizador.Gerente ? "GERENTE" : "OPERADOR DE CAIXA";
    public string NomeEmpresa { get; private set; } = "KiVenda";
    public ObservableCollection<ItemMenuLateral> ItensMenu { get; } = new();
    public ObservableCollection<ItemMenuLateral> ItensMenuInferiores { get; } = new();
    public ObservableCollection<Notificacao> Notificacoes => _servicoNotificacoes.Notificacoes;

    [ObservableProperty] private ItemMenuLateral? _itemSelecionado;
    [ObservableProperty] private ViewModelBase? _conteudoAtual;
    [ObservableProperty] private bool _notificacoesAbertas;

    [ObservableProperty] private bool _mostrarBannerLicenca;
    [ObservableProperty] private string _bannerLicencaTitulo = string.Empty;
    [ObservableProperty] private string _bannerLicencaMensagem = string.Empty;
    [ObservableProperty] private bool _bannerLicencaBloqueado;

    partial void OnItemSelecionadoChanged(ItemMenuLateral? value)
    {
        var politica = PoliticaLicencaKiVenda.Avaliar();
        if (politica.Bloqueado && value?.Nome != "Configurações")
        {
            var configuracoes = ItensMenuInferiores.FirstOrDefault(x => x.Nome == "Configurações");
            if (configuracoes is not null)
            {
                ItemSelecionado = configuracoes;
                return;
            }
        }

        ConteudoAtual = value?.FabricaConteudo();
    }

    public bool TemNotificacoesNaoLidas => _servicoNotificacoes.NaoLidas > 0;
    public int QuantidadeNotificacoes => _servicoNotificacoes.Notificacoes.Count;
    public string TemaBotaoTexto => _servicoTema.TemaAtual == TemaKiVenda.Dark ? "☀" : "☾";

    public event EventHandler? SessaoTerminada;
    public event EventHandler? MeusDadosSolicitados;

    public ShellViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao, ServicoNotificacoes servicoNotificacoes, ServicoTema servicoTema)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        _servicoNotificacoes = servicoNotificacoes;
        _servicoTema = servicoTema;
        _servicoNotificacoes.IniciarAtualizacaoAutomatica();

        _servicoNotificacoes.Alteradas += OnNotificacoesAlteradas;
        Licensing.StatusChanged += OnLicensingStatusChanged;

        _timerLicenca = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timerLicenca.Tick += (_, _) => AtualizarEstadoLicenca();
        _timerLicenca.Start();

        _ = CarregarEmpresaAsync();
        AtualizarEstadoLicenca();
        ConstruirMenu();

        var politicaInicial = PoliticaLicencaKiVenda.Avaliar();
        ItemSelecionado = politicaInicial.Bloqueado
            ? ItensMenuInferiores.FirstOrDefault(x => x.Nome == "Configurações")
            : ItensMenu.FirstOrDefault();
    }

    private async Task CarregarEmpresaAsync()
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var empresa = await scope.ServiceProvider.GetRequiredService<ObterEmpresaUseCase>().ExecutarAsync();
            if (empresa is not null && !string.IsNullOrWhiteSpace(empresa.NomeComercial))
                NomeEmpresa = empresa.NomeComercial;

            OnPropertyChanged(nameof(NomeEmpresa));
        }
        catch
        {
            // A shell continua funcional mesmo sem cadastro de empresa.
        }
    }

    private void ConstruirMenu()
    {
        ItensMenu.Clear();
        ItensMenuInferiores.Clear();

        var politica = PoliticaLicencaKiVenda.Avaliar();

        if (politica.Bloqueado)
        {
            if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
                ItensMenuInferiores.Add(Item("Configurações", "⚙️", CriarConfiguracoes));
            return;
        }

        ItensMenu.Add(Item("Dashboard", "🏠", () => new DashboardViewModel(_scopeFactory, _sessao)));
        ItensMenu.Add(Item("Vendas", "🛒", () => new VendasViewModel(_scopeFactory)));
        ItensMenu.Add(Item("Stock", "📦", () => new StockViewModel(_scopeFactory, _sessao)));

        ItensMenu.Add(Item("Cadastros", "👥", () => new CadastrosViewModel(_scopeFactory)));


        if (Permissoes.Permite(_sessao.Perfil, Acao.GerirCaixa))
            ItensMenu.Add(Item("Caixa", "🏦", () => new CaixaViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.AcederRelatorios))
            ItensMenu.Add(Item("Relatórios", "📊", () => new RelatoriosViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
            ItensMenu.Add(Item("Auditoria", "🛡️", () => new AuditoriaViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.CriarUtilizadores))
            ItensMenuInferiores.Add(Item("Utilizadores", "👤", () => new UtilizadoresViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
            ItensMenuInferiores.Add(Item("Configurações", "⚙️", CriarConfiguracoes));
    }

    private ConfiguracoesViewModel CriarConfiguracoes()
    {
        var configuracoes = App.Services.GetRequiredService<ConfiguracoesViewModel>();
        if (PoliticaLicencaKiVenda.Avaliar().Bloqueado)
            configuracoes.SelecionarLicenca();

        return configuracoes;
    }

    private void AtualizarEstadoLicenca()
    {
        var politica = PoliticaLicencaKiVenda.Avaliar();

        MostrarBannerLicenca = politica.MostrarBanner;
        BannerLicencaTitulo = politica.Titulo;
        BannerLicencaMensagem = politica.Mensagem;
        BannerLicencaBloqueado = politica.Bloqueado;

        if (politica.Bloqueado)
        {
            ConstruirMenu();
            var configuracoes = ItensMenuInferiores.FirstOrDefault(x => x.Nome == "Configurações");
            if (configuracoes is not null)
                ItemSelecionado = configuracoes;
        }
    }

    private void OnLicensingStatusChanged(object? sender, EventArgs e)
    {
        AtualizarEstadoLicenca();
        ConstruirMenu();

        var politica = PoliticaLicencaKiVenda.Avaliar();
        ItemSelecionado = politica.Bloqueado
            ? ItensMenuInferiores.FirstOrDefault(x => x.Nome == "Configurações")
            : ItensMenu.FirstOrDefault();
    }

    private void OnNotificacoesAlteradas(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(TemNotificacoesNaoLidas));
        OnPropertyChanged(nameof(Notificacoes));
        OnPropertyChanged(nameof(QuantidadeNotificacoes));
    }

    [RelayCommand]
    private void SelecionarProdutos() => ItemSelecionado = Item("Produtos", "📦", () => new ProdutosViewModel(_scopeFactory, _sessao));

    [RelayCommand]
    private void SelecionarAbastecer() => ItemSelecionado = Item("Abastecer", "🧾", () => new ComprasViewModel(_scopeFactory));

    [RelayCommand]
    private void AlternarTema()
    {
        var proximo = _servicoTema.TemaAtual == TemaKiVenda.Dark ? TemaKiVenda.Light : TemaKiVenda.Dark;
        _servicoTema.DefinirTema(proximo);
        OnPropertyChanged(nameof(TemaBotaoTexto));
    }

    [RelayCommand]
    private async Task EliminarNotificacaoAsync(Notificacao notificacao)
    {
        await _servicoNotificacoes.RemoverAsync(notificacao.Id);
    }

    [RelayCommand]
    private void SelecionarItem(ItemMenuLateral item)
    {
        ItemSelecionado = item;
    }

    [RelayCommand]
    private async Task AbrirNotificacoesAsync()
    {
        NotificacoesAbertas = !NotificacoesAbertas;
        if (NotificacoesAbertas)
            await _servicoNotificacoes.MarcarTodasComoLidasAsync();
    }

    [RelayCommand]
    private void FecharNotificacoes() => NotificacoesAbertas = false;

    [RelayCommand]
    private void MeusDados() => MeusDadosSolicitados?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void TerminarSessao()
    {
        Licensing.StatusChanged -= OnLicensingStatusChanged;
        _servicoNotificacoes.Alteradas -= OnNotificacoesAlteradas;
        _servicoNotificacoes.PararAtualizacaoAutomatica();
        _timerLicenca.Stop();
        _timerLicenca.Tick -= (_, _) => AtualizarEstadoLicenca();
        _sessao.TerminarSessao();
        SessaoTerminada?.Invoke(this, EventArgs.Empty);
    }

    private static string ObterIniciais(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return "?";
        if (partes.Length == 1) return partes[0][..1].ToUpperInvariant();
        return string.Concat(partes[0][0], partes[^1][0]).ToUpperInvariant();
    }

    private static ItemMenuLateral Item(string nome, string icone, Func<ViewModelBase> fabrica) =>
        new() { Nome = nome, Icone = icone, FabricaConteudo = fabrica };
}
