using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Empresas;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.Notificacoes;
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

    public bool TemNotificacoesNaoLidas => _servicoNotificacoes.NaoLidas > 0;

    public event EventHandler? SessaoTerminada;
    public event EventHandler? MeusDadosSolicitados;

    public ShellViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao, ServicoNotificacoes servicoNotificacoes)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        _servicoNotificacoes = servicoNotificacoes;

        _servicoNotificacoes.Alteradas += OnNotificacoesAlteradas;
        Licensing.StatusChanged += OnLicensingStatusChanged;
        _ = CarregarEmpresaAsync();
        ConstruirMenu();
        ItemSelecionado = ItensMenu.FirstOrDefault();
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

        if (OperatingSystem.IsWindows() && Licensing.CurrentStatus != LicenseStatus.Valid)
        {
            if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
                ItensMenuInferiores.Add(Item("Configurações", "⚙️", CriarConfiguracoes));
            return;
        }

        ItensMenu.Add(Item("Dashboard", "🏠", () => new DashboardViewModel(_scopeFactory, _sessao)));
        ItensMenu.Add(Item("Vendas", "🛒", () => new VendasViewModel(_scopeFactory)));
        ItensMenu.Add(Item("Produtos", "📦", () => new ProdutosViewModel(_scopeFactory, _sessao)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.RegistarCompras))
            ItensMenu.Add(Item("Compras", "🧾", () => new ComprasViewModel(_scopeFactory)));

        ItensMenu.Add(Item("Clientes", "👥", () => new ClientesViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.RegistarCompras))
            ItensMenu.Add(Item("Fornecedores", "🚚", () => new FornecedoresViewModel(_scopeFactory)));

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

    private static ConfiguracoesViewModel CriarConfiguracoes() =>
        App.Services.GetRequiredService<ConfiguracoesViewModel>();

    private void OnLicensingStatusChanged(object? sender, EventArgs e)
    {
        ConstruirMenu();
        ItemSelecionado = ItensMenu.FirstOrDefault();
    }

    private void OnNotificacoesAlteradas(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(TemNotificacoesNaoLidas));
        OnPropertyChanged(nameof(Notificacoes));
    }

    [RelayCommand]
    private void SelecionarItem(ItemMenuLateral item)
    {
        ItemSelecionado = item;
    }

    [RelayCommand]
    private void AbrirNotificacoes()
    {
        NotificacoesAbertas = !NotificacoesAbertas;
        if (NotificacoesAbertas)
            _servicoNotificacoes.MarcarTodasComoLidas();
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
