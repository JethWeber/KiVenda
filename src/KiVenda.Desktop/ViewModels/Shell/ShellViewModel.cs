using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Core.Enums;
using KiVenda.Core.Utilizadores;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Desktop.ViewModels.Modulos;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Shell;

/// <summary>
/// Anfitrião da aplicação depois do login: sidebar com os 10 módulos
/// (Secção 8 da documentação funcional) + área de conteúdo que troca
/// consoante o item selecionado. Cada item só aparece se o perfil da
/// sessão atual tiver a permissão correspondente — a mesma matriz
/// (<see cref="Permissoes"/>) usada pelos casos de uso, nunca duplicada
/// aqui.
/// </summary>
public partial class ShellViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessaoUtilizadorAtual _sessao;

    public string NomeUtilizador => _sessao.Nome;

    public string InicialNome => string.IsNullOrEmpty(_sessao.Nome) ? "?" : _sessao.Nome[..1].ToUpperInvariant();

    public string PerfilTexto => _sessao.Perfil == PerfilUtilizador.Gerente ? "Gerente" : "Atendente";

    public ObservableCollection<ItemMenuLateral> ItensMenu { get; } = new();

    [ObservableProperty]
    private ItemMenuLateral? _itemSelecionado;

    [ObservableProperty]
    private ViewModelBase? _conteudoAtual;

    public event EventHandler? SessaoTerminada;

    public ShellViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;

        ConstruirMenu();
        ItemSelecionado = ItensMenu.FirstOrDefault();
    }

    private void ConstruirMenu()
    {
        ItensMenu.Add(Item("Dashboard", "🏠", () => new DashboardViewModel(_scopeFactory, _sessao)));

        // Vendas e Caixa: implementação real na Fase 7 — item já visível
        // (fiel aos mockups), mas com conteúdo placeholder por agora.
        ItensMenu.Add(Item("Vendas", "🛒", () => new EmBreveViewModel("Vendas", "Fase 7")));

        ItensMenu.Add(Item("Produtos", "📦", () => new ProdutosViewModel(_scopeFactory, _sessao)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.RegistarCompras))
        {
            ItensMenu.Add(Item("Compras", "🧾", () => new ComprasViewModel(_scopeFactory)));
        }

        ItensMenu.Add(Item("Clientes", "👥", () => new ClientesViewModel(_scopeFactory)));

        if (Permissoes.Permite(_sessao.Perfil, Acao.RegistarCompras))
        {
            ItensMenu.Add(Item("Fornecedores", "🚚", () => new FornecedoresViewModel(_scopeFactory)));
        }

        if (Permissoes.Permite(_sessao.Perfil, Acao.GerirCaixa))
        {
            ItensMenu.Add(Item("Caixa", "🏦", () => new EmBreveViewModel("Caixa", "Fase 7")));
        }

        if (Permissoes.Permite(_sessao.Perfil, Acao.AcederRelatorios))
        {
            ItensMenu.Add(Item("Relatórios", "📊", () => new EmBreveViewModel("Relatórios", "Fase 9")));
        }

        if (Permissoes.Permite(_sessao.Perfil, Acao.CriarUtilizadores))
        {
            ItensMenu.Add(Item("Utilizadores", "👤", () => new UtilizadoresViewModel(_scopeFactory)));
        }

        if (Permissoes.Permite(_sessao.Perfil, Acao.ConfigurarSistema))
        {
            ItensMenu.Add(Item("Configurações", "⚙️", () => new EmBreveViewModel("Configurações", "Fase 11")));
        }
    }

    private static ItemMenuLateral Item(string nome, string icone, Func<ViewModelBase> fabrica) =>
        new() { Nome = nome, Icone = icone, FabricaConteudo = fabrica };

    partial void OnItemSelecionadoChanged(ItemMenuLateral? value)
    {
        if (value is not null)
        {
            ConteudoAtual = value.FabricaConteudo();
        }
    }

    [RelayCommand]
    private void TerminarSessao()
    {
        _sessao.TerminarSessao();
        SessaoTerminada?.Invoke(this, EventArgs.Empty);
    }
}
