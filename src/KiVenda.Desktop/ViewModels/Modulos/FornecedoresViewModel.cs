using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Fornecedores;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class FornecedoresViewModel : ListaModuloViewModelBase<FornecedorDto>
{
    [ObservableProperty]
    private bool _formularioAberto;

    [ObservableProperty]
    private string _novoNome = string.Empty;

    [ObservableProperty]
    private string _novoTelefone = string.Empty;

    [ObservableProperty]
    private string _novosProdutosFornecidos = string.Empty;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private bool _aGuardar;

    public FornecedoresViewModel(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _ = CarregarAsync();
    }

    protected override async Task<IReadOnlyList<FornecedorDto>> ObterItensAsync(IServiceProvider servicos)
    {
        var useCase = servicos.GetRequiredService<ListarFornecedoresUseCase>();
        return await useCase.ExecutarAsync(new ListarFornecedoresQuery(TermoPesquisa));
    }

    [RelayCommand]
    private void AbrirFormulario() => FormularioAberto = true;

    [RelayCommand]
    private void FecharFormulario()
    {
        FormularioAberto = false;
        NovoNome = string.Empty;
        NovoTelefone = string.Empty;
        NovosProdutosFornecidos = string.Empty;
        MensagemErroFormulario = null;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        MensagemErroFormulario = null;

        if (string.IsNullOrWhiteSpace(NovoNome))
        {
            MensagemErroFormulario = "O nome é obrigatório.";
            return;
        }

        AGuardar = true;
        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<CriarFornecedorUseCase>();

            await useCase.ExecutarAsync(new CriarFornecedorCommand(
                NovoNome,
                string.IsNullOrWhiteSpace(NovoTelefone) ? null : NovoTelefone,
                string.IsNullOrWhiteSpace(NovosProdutosFornecidos) ? null : NovosProdutosFornecidos));

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
