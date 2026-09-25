using System.Collections.ObjectModel;
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
    private readonly SessaoUtilizadorAtual _sessao;

    public string NomeUtilizador { get; }

    public string SaudacaoAtual
    {
        get
        {
            var hora = DateTime.Now.Hour;
            return hora switch
            {
                >= 1 and <= 4 => "Boa madrugada",
                >= 5 and <= 11 => "Bom dia",
                >= 12 and <= 17 => "Boa tarde",
                _ => "Boa noite"
            };
        }
    }

    public string DataAtualTexto => DateTime.Now.ToString(
        "dddd, dd 'de' MMMM 'de' yyyy",
        new System.Globalization.CultureInfo("pt-AO"));

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

    [ObservableProperty]
    private bool _alertasPopupAberto;

    public ObservableCollection<AlertaDashboard> Alertas { get; } = new();

    public bool TemAlertas => Alertas.Count > 0;

    public DashboardViewModel(IServiceScopeFactory scopeFactory, SessaoUtilizadorAtual sessao)
    {
        _scopeFactory = scopeFactory;
        _sessao = sessao;
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
            CaixaAtualTexto = resumo.CaixaAtual is null
                ? "Caixa fechado"
                : FormatadorKz.Formatar(resumo.CaixaAtual.Value);

            Alertas.Clear();

            // Stock baixo/sem stock é sempre o primeiro alerta do dashboard.
            if (resumo.ProdutosStockBaixoOuSemStock > 0)
            {
                Alertas.Add(new AlertaDashboard(
                    null,
                    "stock",
                    "Stock Crítico",
                    $"Existem {resumo.ProdutosStockBaixoOuSemStock} produtos com stock baixo ou sem stock.",
                    true));
            }

            foreach (var alerta in resumo.Alertas)
            {
                Alertas.Add(new AlertaDashboard(
                    alerta.Id,
                    alerta.Tipo,
                    alerta.Titulo,
                    alerta.Mensagem,
                    true));
            }

            if (CaixaFechado)
            {
                Alertas.Add(new AlertaDashboard(
                    null,
                    "caixa",
                    "Sessão de Caixa",
                    "Não existe uma sessão de caixa aberta neste momento.",
                    true));
            }
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

    [RelayCommand]
    private void AbrirAlertas()
    {
        AlertasPopupAberto = true;
    }

    [RelayCommand]
    private void FecharAlertas()
    {
        AlertasPopupAberto = false;
    }

    [RelayCommand]
    private async Task RemoverAlertaAsync(AlertaDashboard? alerta)
    {
        if (alerta is null)
        {
            return;
        }

        Alertas.Remove(alerta);\n        OnPropertyChanged(nameof(TemAlertas));

        if (alerta.Id is not Guid notificacaoId)
        {
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var uow = scope.ServiceProvider.GetRequiredService<KiVenda.Application.Abstractions.Persistence.IUnitOfWork>();
            await uow.Notificacoes.RemoverAsync(notificacaoId, _sessao.UtilizadorId);
            await uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
    }
}

public sealed record AlertaDashboard(
    Guid? Id,
    string Tipo,
    string Titulo,
    string Mensagem,
    bool PodeEliminar);
