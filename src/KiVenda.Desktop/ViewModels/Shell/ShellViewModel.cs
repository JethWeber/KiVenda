using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using KiVenda.Desktop.Autenticacao;
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

    public string NomeUtilizador => _sessao.Nome;
    public string InicialNome => string.IsNullOrEmpty(_sessao.Nome) ? "?" : _sessao.Nome[..1].ToUpperInvariant();
    public string PerfilTexto => _sessao.Perfil == PerfilUtilizador.Gerente ? "Gerente" : "Atendente";
    public ObservableCollection<ItemMenuLateral> ItensMenu { get; } = new();

    [ObservableProperty] private ItemMenuLateral? _itemSelecionado;
    [ObservableProperty] private ViewModelBase? _conteudoAtual;

    public event EventHandler? SessaoTerminada;

    public ShellViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
        Licensing.StatusChanged += OnLicensingStatusChanged;
        ConstruirMenu();
        ItemSelecionado = ItensMenu.FirstOrDefault();
    }

    private void ConstruirMenu()
    {
        ItensMenu.Clear();

        if (OperatingSystem.IsWindows() && Licensing.CurrentStatus != LicenseStatus.Valid)
        {
            if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
                ItensMenu.Add(Item("Configurações", "⚙️", CriarConfiguracoes));
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

        if (Permissoes.Permite(_sessao.Perfil, Acao.CriarUtilizadores))
            ItensMenu.Add(Item("Utilizadores", "👤", () => new UtilizadoresViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
        {
            ItensMenu.Add(Item("Auditoria", "🛡️", () => new AuditoriaViewModel(_scopeFactory)));
            ItensMenu.Add(Item("Configurações", "⚙️", CriarConfiguracoes));
        }
    }

    private static ConfiguracoesViewModel CriarConfiguracoes() =>
        App.Services.GetRequiredService<ConfiguracoesViewModel>();

    private void OnLicensingStatusChanged(object? sender, EventArgs e)
    {
        ConstruirMenu();
        ItemSelecionado = ItensMenu.FirstOrDefault();
    }

    private static ItemMenuLateral Item(string nome, string icone, Func<ViewModelBase> fabrica) =>
        new() { Nome = nome, Icone = icone, FabricaConteudo = fabrica };

    partial void OnItemSelecionadoChanged(ItemMenuLateral? value)
    {
        if (value is not null)
            ConteudoAtual = value.FabricaConteudo();
    }

    [RelayCommand]
    private void TerminarSessao()
    {
        Licensing.StatusChanged -= OnLicensingStatusChanged;
        _sessao.TerminarSessao();
        SessaoTerminada?.Invoke(this, EventArgs.Empty);
    }
}
