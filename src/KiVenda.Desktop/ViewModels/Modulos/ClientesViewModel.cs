using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Clientes;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class ClientesViewModel : ListaModuloViewModelBase<ClienteDto>
{
    [ObservableProperty]
    private bool _formularioAberto;

    [ObservableProperty]
    private string _novoNome = string.Empty;

    [ObservableProperty]
    private string _novoTelefone = string.Empty;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private bool _aGuardar;

    public ClientesViewModel(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _ = CarregarAsync();
    }

    protected override async Task<IReadOnlyList<ClienteDto>> ObterItensAsync(IServiceProvider servicos)
    {
        var useCase = servicos.GetRequiredService<ListarClientesUseCase>();
        return await useCase.ExecutarAsync(new ListarClientesQuery(TermoPesquisa));
    }

    [RelayCommand]
    private void AbrirFormulario() => FormularioAberto = true;

    [RelayCommand]
    private void FecharFormulario()
    {
        FormularioAberto = false;
        NovoNome = string.Empty;
        NovoTelefone = string.Empty;
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
            var useCase = scope.ServiceProvider.GetRequiredService<CriarClienteUseCase>();

            await useCase.ExecutarAsync(new CriarClienteCommand(NovoNome, string.IsNullOrWhiteSpace(NovoTelefone) ? null : NovoTelefone));

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
