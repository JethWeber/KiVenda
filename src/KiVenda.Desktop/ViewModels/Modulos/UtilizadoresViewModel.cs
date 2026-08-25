using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Enums;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Gestão de Utilizadores (Secção 5) — restrito ao Gerente
/// (<see cref="Acao.CriarUtilizadores"/>), reforçado tanto na
/// visibilidade deste item no menu (Fase 5/6) como na verificação de
/// permissão dentro de cada caso de uso (Fase 3).
/// </summary>
public partial class UtilizadoresViewModel : ListaModuloViewModelBase<UtilizadorDto>
{
    [ObservableProperty]
    private bool _formularioAberto;

    [ObservableProperty]
    private string _novoNome = string.Empty;

    [ObservableProperty]
    private string _novoNomeUtilizador = string.Empty;

    [ObservableProperty]
    private string _novaSenha = string.Empty;

    [ObservableProperty]
    private bool _novoPerfilGerente;

    [ObservableProperty]
    private string? _mensagemErroFormulario;

    [ObservableProperty]
    private bool _aGuardar;

    public UtilizadoresViewModel(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _ = CarregarAsync();
    }

    protected override async Task<IReadOnlyList<UtilizadorDto>> ObterItensAsync(IServiceProvider servicos)
    {
        var useCase = servicos.GetRequiredService<ListarUtilizadoresUseCase>();
        return await useCase.ExecutarAsync(new ListarUtilizadoresQuery());
    }

    [RelayCommand]
    private void AbrirFormulario() => FormularioAberto = true;

    [RelayCommand]
    private void FecharFormulario()
    {
        FormularioAberto = false;
        NovoNome = string.Empty;
        NovoNomeUtilizador = string.Empty;
        NovaSenha = string.Empty;
        NovoPerfilGerente = false;
        MensagemErroFormulario = null;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        MensagemErroFormulario = null;

        if (string.IsNullOrWhiteSpace(NovoNome) || string.IsNullOrWhiteSpace(NovoNomeUtilizador) || string.IsNullOrWhiteSpace(NovaSenha))
        {
            MensagemErroFormulario = "Preencha nome, utilizador e password.";
            return;
        }

        AGuardar = true;
        try
        {
            await using var scope = ScopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<CriarUtilizadorUseCase>();

            var perfil = NovoPerfilGerente ? PerfilUtilizador.Gerente : PerfilUtilizador.Atendente;
            await useCase.ExecutarAsync(new CriarUtilizadorCommand(NovoNome, NovoNomeUtilizador, NovaSenha, perfil));

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
