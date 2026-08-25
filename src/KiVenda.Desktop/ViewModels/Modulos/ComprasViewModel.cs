using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Compras;
using KiVenda.Application.Fornecedores;
using KiVenda.Application.Produtos;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Registo de Compras (Secção 4.3). Formulário simplificado: um item
/// por compra de cada vez (a apresentação comprada usa sempre a
/// apresentação padrão do produto nesta fase — selecionar entre várias
/// apresentações fica para um refinamento futuro deste ecrã).
/// </summary>
public partial class ComprasViewModel : ListaModuloViewModelBase<CompraDto>
{
    [ObservableProperty]
    private bool _formularioAberto;

    [ObservableProperty]
    private FornecedorDto? _fornecedorSelecionado;

    [ObservableProperty]
    private ProdutoDto? _produtoSelecionado;

    [ObservableProperty]
    private string _quantidade = string.Empty;

    [ObservableProperty]
    private string _custoTotal = string.Empty;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private bool _aGuardar;

    public ObservableCollection<FornecedorDto> Fornecedores { get; } = new();

    public ObservableCollection<ProdutoDto> Produtos { get; } = new();

    public ComprasViewModel(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _ = CarregarAsync();
        _ = CarregarListasAuxiliaresAsync();
    }

    protected override async Task<IReadOnlyList<CompraDto>> ObterItensAsync(IServiceProvider servicos)
    {
        var useCase = servicos.GetRequiredService<ListarComprasUseCase>();
        return await useCase.ExecutarAsync(new ListarComprasQuery());
    }

    private async Task CarregarListasAuxiliaresAsync()
    {
        await using var scope = ScopeFactory.CreateAsyncScope();

        var fornecedores = await scope.ServiceProvider.GetRequiredService<ListarFornecedoresUseCase>().ExecutarAsync(new ListarFornecedoresQuery());
        Fornecedores.Clear();
        foreach (var fornecedor in fornecedores)
        {
            Fornecedores.Add(fornecedor);
        }

        var produtos = await scope.ServiceProvider.GetRequiredService<ListarProdutosUseCase>().ExecutarAsync(new ListarProdutosQuery());
        Produtos.Clear();
        foreach (var produto in produtos)
        {
            Produtos.Add(produto);
        }
    }

    [RelayCommand]
    private void AbrirFormulario() => FormularioAberto = true;

    [RelayCommand]
    private void FecharFormulario()
    {
        FormularioAberto = false;
        Quantidade = string.Empty;
        CustoTotal = string.Empty;
        MensagemErroFormulario = null;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        MensagemErroFormulario = null;

        if (FornecedorSelecionado is null || ProdutoSelecionado is null)
        {
            MensagemErroFormulario = "Escolha o fornecedor e o produto.";
            return;
        }

        var apresentacaoPadrao = ProdutoSelecionado.Apresentacoes.FirstOrDefault(a => a.Ativa);
        if (apresentacaoPadrao is null)
        {
            MensagemErroFormulario = "Este produto não tem nenhuma apresentação ativa.";
            return;
        }

        if (!decimal.TryParse(Quantidade, out var quantidade) || quantidade <= 0)
        {
            MensagemErroFormulario = "Quantidade inválida.";
            return;
        }

        if (!decimal.TryParse(CustoTotal, out var custoTotal) || custoTotal < 0)
        {
            MensagemErroFormulario = "Custo total inválido.";
            return;
        }

        AGuardar = true;
        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RegistarCompraUseCase>();

            var item = new ItemCompraCommand(ProdutoSelecionado.Id, apresentacaoPadrao.Id, quantidade, custoTotal);
            await useCase.ExecutarAsync(new RegistarCompraCommand(FornecedorSelecionado.Id, new[] { item }));

            FecharFormularioCommand.Execute(null);
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
}
