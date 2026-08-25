using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Produtos;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Core.Exceptions;
using KiVenda.Core.Utilizadores;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Gestão de Inventário (Secção 4.2). Lista + cadastro básico — a
/// gestão de apresentações comerciais adicionais (Fase 1: "1 kg",
/// "Saco 25 kg") fica para um refinamento futuro deste ecrã; o produto
/// já nasce com a apresentação padrão (fator 1), suficiente para
/// operar desde já.
/// </summary>
public partial class ProdutosViewModel : ListaModuloViewModelBase<ProdutoDto>
{
    public bool PodeCriar { get; }

    [ObservableProperty]
    private bool _formularioAberto;

    [ObservableProperty]
    private string _novoNome = string.Empty;

    [ObservableProperty]
    private string _novoCodigoInterno = string.Empty;

    [ObservableProperty]
    private string _novoCodigoBarras = string.Empty;

    [ObservableProperty]
    private string _novoPrecoVenda = string.Empty;

    [ObservableProperty]
    private string _novoStockMinimo = string.Empty;

    [ObservableProperty]
    private CategoriaDto? _categoriaSelecionada;

    [ObservableProperty]
    private UnidadeMedidaDto? _unidadeSelecionada;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private bool _aGuardar;

    public ObservableCollection<CategoriaDto> Categorias { get; } = new();

    public ObservableCollection<UnidadeMedidaDto> Unidades { get; } = new();

    public ProdutosViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao) : base(scopeFactory)
    {
        PodeCriar = Permissoes.Permite(sessao.Perfil, Acao.CadastrarProdutos);

        _ = CarregarAsync();
        _ = CarregarListasAuxiliaresAsync();
    }

    protected override async Task<IReadOnlyList<ProdutoDto>> ObterItensAsync(IServiceProvider servicos)
    {
        var useCase = servicos.GetRequiredService<ListarProdutosUseCase>();
        return await useCase.ExecutarAsync(new ListarProdutosQuery(TermoPesquisa));
    }

    private async Task CarregarListasAuxiliaresAsync()
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var categorias = await scope.ServiceProvider.GetRequiredService<ListarCategoriasUseCase>().ExecutarAsync();
        Categorias.Clear();
        foreach (var categoria in categorias)
        {
            Categorias.Add(categoria);
        }

        var unidades = await scope.ServiceProvider.GetRequiredService<ListarUnidadesMedidaUseCase>().ExecutarAsync();
        Unidades.Clear();
        foreach (var unidade in unidades)
        {
            Unidades.Add(unidade);
        }

        CategoriaSelecionada ??= Categorias.FirstOrDefault();
        UnidadeSelecionada ??= Unidades.FirstOrDefault();
    }

    [RelayCommand]
    private void AbrirFormulario() => FormularioAberto = true;

    [RelayCommand]
    private void FecharFormulario()
    {
        FormularioAberto = false;
        LimparFormulario();
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        MensagemErroFormulario = null;

        if (string.IsNullOrWhiteSpace(NovoNome) || string.IsNullOrWhiteSpace(NovoCodigoInterno))
        {
            MensagemErroFormulario = "Nome e código são obrigatórios.";
            return;
        }

        if (CategoriaSelecionada is null || UnidadeSelecionada is null)
        {
            MensagemErroFormulario = "Escolha uma categoria e uma unidade de medida.";
            return;
        }

        if (!decimal.TryParse(NovoPrecoVenda, out var precoVenda) || precoVenda < 0)
        {
            MensagemErroFormulario = "Preço de venda inválido.";
            return;
        }

        if (!decimal.TryParse(NovoStockMinimo, out var stockMinimo) || stockMinimo < 0)
        {
            MensagemErroFormulario = "Stock mínimo inválido.";
            return;
        }

        AGuardar = true;
        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<CriarProdutoUseCase>();

            await useCase.ExecutarAsync(new CriarProdutoCommand(
                NovoNome,
                NovoCodigoInterno,
                CategoriaSelecionada.Id,
                UnidadeSelecionada.Id,
                precoVenda,
                stockMinimo,
                string.IsNullOrWhiteSpace(NovoCodigoBarras) ? null : NovoCodigoBarras));

            FormularioAberto = false;
            LimparFormulario();
            await CarregarAsync();
        }
        catch (DomainException ex)
        {
            MensagemErroFormulario = ex.Message;
        }
        finally
        {
            AGuardar = false;
        }
    }

    private void LimparFormulario()
    {
        NovoNome = string.Empty;
        NovoCodigoInterno = string.Empty;
        NovoCodigoBarras = string.Empty;
        NovoPrecoVenda = string.Empty;
        NovoStockMinimo = string.Empty;
        MensagemErroFormulario = null;
    }
}
