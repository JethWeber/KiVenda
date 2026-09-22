using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Auditoria;
using KiVenda.Application.Utilizadores;
using KiVenda.Core.Auditoria;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class AuditoriaViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    [ObservableProperty] private DateTimeOffset? _dataDe = DateTimeOffset.Now.Date;
    [ObservableProperty] private DateTimeOffset? _dataAte = DateTimeOffset.Now.Date;
    [ObservableProperty] private UtilizadorDto? _utilizadorSelecionado;
    [ObservableProperty] private string _entidadeSelecionada = "Todas";
    [ObservableProperty] private string _acao = string.Empty;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private string? _mensagem;

    public ObservableCollection<LogAuditoria> Registos { get; } = new();
    public ObservableCollection<UtilizadorDto> Utilizadores { get; } = new();

    public IReadOnlyList<string> Entidades { get; } =
        ["Todas", "Produto", "Utilizador", "SessaoCaixa", "Venda"];

    public AuditoriaViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _ = InicializarAsync();
    }

    [RelayCommand]
    private async Task InicializarAsync()
    {
        await CarregarUtilizadoresAsync();
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        ACarregar = true;
        Mensagem = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ConsultarAuditoriaUseCase>();

            var de = DataDe?.Date.Date;
            var ate = DataAte?.Date.Date.AddDays(1).AddTicks(-1);

            var registos = await useCase.ExecutarAsync(
                new ConsultarAuditoriaQuery(
                    UtilizadorId: UtilizadorSelecionado?.Id,
                    EntidadeAfetada: EntidadeSelecionada == "Todas" ? null : EntidadeSelecionada,
                    Acao: string.IsNullOrWhiteSpace(Acao) ? null : Acao.Trim(),
                    De: de,
                    Ate: ate));

            Registos.Clear();
            foreach (var registo in registos)
                Registos.Add(registo);

            Mensagem = Registos.Count == 0
                ? "Nenhuma operação encontrada com estes filtros."
                : $"{Registos.Count} operação(ões) encontrada(s).";
        }
        catch (Exception ex)
        {
            Mensagem = ex.Message;
        }
        finally
        {
            ACarregar = false;
        }
    }

    private async Task CarregarUtilizadoresAsync()
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ListarUtilizadoresUseCase>();
            var utilizadores = await useCase.ExecutarAsync(new ListarUtilizadoresQuery());

            Utilizadores.Clear();
            foreach (var utilizador in utilizadores)
                Utilizadores.Add(utilizador);
        }
        catch (Exception ex)
        {
            Mensagem = ex.Message;
        }
    }

    partial void OnDataDeChanged(DateTimeOffset? value) => _ = CarregarAsync();
    partial void OnDataAteChanged(DateTimeOffset? value) => _ = CarregarAsync();
    partial void OnUtilizadorSelecionadoChanged(UtilizadorDto? value) => _ = CarregarAsync();
    partial void OnEntidadeSelecionadaChanged(string value) => _ = CarregarAsync();

    [RelayCommand]
    private void LimparFiltros()
    {
        DataDe = null;
        DataAte = null;
        UtilizadorSelecionado = null;
        EntidadeSelecionada = "Todas";
        Acao = string.Empty;
    }
}
