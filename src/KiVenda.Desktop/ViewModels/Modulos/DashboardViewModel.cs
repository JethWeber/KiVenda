using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Relatorios;
using KiVenda.Desktop.Autenticacao;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Ecrã inicial da aplicação (Secção 4.1): responde de imediato a
/// "Quanto vendi hoje?", sem gráficos elaborados — apenas os
/// indicadores essenciais.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    public string NomeUtilizador { get; }

    [ObservableProperty]
    private bool _aCarregar;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string _vendasDeHojeTexto = "—";

    [ObservableProperty]
    private string _caixaAtualTexto = "—";

    [ObservableProperty]
    private string _lucroEstimadoTexto = "—";

    [ObservableProperty]
    private string _stockBaixoTexto = "—";

    [ObservableProperty]
    private string _vendasRealizadasTexto = "—";

    [ObservableProperty]
    private bool _caixaFechado;

    public DashboardViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        NomeUtilizador = sessao.Nome;

        _ = CarregarAsync();
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        ACarregar = true;
        MensagemErro = null;

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<ObterResumoDashboardUseCase>();
            var resumo = await useCase.ExecutarAsync();

            VendasDeHojeTexto = FormatadorKz.Formatar(resumo.VendasDeHoje);
            LucroEstimadoTexto = FormatadorKz.Formatar(resumo.LucroEstimadoHoje);
            StockBaixoTexto = resumo.ProdutosStockBaixoOuSemStock.ToString();
            VendasRealizadasTexto = resumo.VendasRealizadasHoje.ToString();

            CaixaFechado = resumo.CaixaAtual is null;
            CaixaAtualTexto = resumo.CaixaAtual is null ? "Caixa fechado" : FormatadorKz.Formatar(resumo.CaixaAtual.Value);
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            ACarregar = false;
        }
    }
}
