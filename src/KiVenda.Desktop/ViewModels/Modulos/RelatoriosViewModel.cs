using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Relatorios;
using KiVenda.Application.Utilizadores;
using KiVenda.Desktop.ViewModels.Common;
using KiVenda.Infrastructure.Impressao;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

public partial class RelatoriosViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServicoImpressao _impressao;

    [ObservableProperty] private DateTimeOffset? _dataSelecionada = DateTimeOffset.Now;
    [ObservableProperty] private string _relatorioSelecionado = "Diario";

    public bool MostrarDiario => RelatorioSelecionado == "Diario";
    public bool MostrarMensal => RelatorioSelecionado == "Mensal";
    public bool MostrarStock => RelatorioSelecionado == "Stock";
    public bool MostrarFiltrosData => !MostrarStock;
    public bool MostrarFiltroUtilizador => MostrarDiario;
    [ObservableProperty] private bool _aCarregar;
    [ObservableProperty] private string? _mensagem;
    [ObservableProperty] private RelatorioDiarioDto? _diario;
    [ObservableProperty] private RelatorioMensalDto? _mensal;
    [ObservableProperty] private RelatorioStockDto? _stock;
    [ObservableProperty] private UtilizadorDto? _utilizadorSelecionado;

    public ObservableCollection<UtilizadorDto> Utilizadores { get; } = new();

    public RelatoriosViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _impressao = App.Services.GetRequiredService<IServicoImpressao>();
        _ = InicializarAsync();
    }

    [RelayCommand]
    private async Task InicializarAsync()
    {
        await CarregarUtilizadoresAsync();
        await CarregarRelatorioAsync();
    }

    [RelayCommand]
    private async Task CarregarRelatorioAsync()
    {
        ACarregar = true;
        Mensagem = null;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            switch (RelatorioSelecionado)
            {
                case "Diario":
                    var data = DateOnly.FromDateTime((DataSelecionada ?? DateTimeOffset.Now).Date);
                    var diarioUseCase = scope.ServiceProvider.GetRequiredService<GerarRelatorioDiarioUseCase>();
                    Diario = await diarioUseCase.ExecutarAsync(
                        new GerarRelatorioDiarioQuery(data, UtilizadorSelecionado?.Id));
                    break;

                case "Mensal":
                    var mes = DataSelecionada ?? DateTimeOffset.Now;
                    var mensalUseCase = scope.ServiceProvider.GetRequiredService<GerarRelatorioMensalUseCase>();
                    Mensal = await mensalUseCase.ExecutarAsync(
                        new GerarRelatorioMensalQuery(mes.Year, mes.Month));
                    break;

                case "Stock":
                    var stockUseCase = scope.ServiceProvider.GetRequiredService<GerarRelatorioStockUseCase>();
                    Stock = await stockUseCase.ExecutarAsync();
                    break;
            }
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

    [RelayCommand]
    private async Task ImprimirAsync()
    {
        try
        {
            var titulo = $"Relatório {RelatorioSelecionado}";
            var conteudo = ConstruirTextoRelatorio();
            await _impressao.ImprimirTextoAsync(titulo, conteudo);
            Mensagem = "Relatório enviado para impressão.";
        }
        catch (Exception ex)
        {
            Mensagem = $"Não foi possível imprimir o relatório: {ex.Message}";
        }
    }

    partial void OnRelatorioSelecionadoChanged(string value)
    {
        OnPropertyChanged(nameof(MostrarDiario));
        OnPropertyChanged(nameof(MostrarMensal));
        OnPropertyChanged(nameof(MostrarStock));
        OnPropertyChanged(nameof(MostrarFiltrosData));
        OnPropertyChanged(nameof(MostrarFiltroUtilizador));
        _ = CarregarRelatorioAsync();
    }
    partial void OnDataSelecionadaChanged(DateTimeOffset? value)
    {
        if (RelatorioSelecionado is "Diario" or "Mensal")
            _ = CarregarRelatorioAsync();
    }
    [RelayCommand]
    private void SelecionarDiario() => RelatorioSelecionado = "Diario";

    [RelayCommand]
    private void SelecionarMensal() => RelatorioSelecionado = "Mensal";

    [RelayCommand]
    private void SelecionarStock() => RelatorioSelecionado = "Stock";

    partial void OnUtilizadorSelecionadoChanged(UtilizadorDto? value)
    {
        if (RelatorioSelecionado == "Diario")
            _ = CarregarRelatorioAsync();
    }

    private string ConstruirTextoRelatorio()
    {
        var sb = new StringBuilder();
        sb.AppendLine("KIVENDA");
        sb.AppendLine($"RELATÓRIO {RelatorioSelecionado.ToUpperInvariant()}");
        sb.AppendLine(new string('=', 42));

        switch (RelatorioSelecionado)
        {
            case "Diario" when Diario is not null:
                sb.AppendLine($"Data: {Diario.Data:dd/MM/yyyy}");
                sb.AppendLine($"Vendas: {Diario.NumeroDeVendas}");
                sb.AppendLine($"Total vendido: {FormatadorKz.Formatar(Diario.TotalVendido)}");
                sb.AppendLine($"Lucro estimado: {FormatadorKz.Formatar(Diario.LucroEstimado)}");
                sb.AppendLine();
                sb.AppendLine("PRODUTOS VENDIDOS");
                foreach (var item in Diario.ProdutosVendidos)
                    sb.AppendLine($"{item.ProdutoNome} | {item.QuantidadeUnidadeBase:N2} un.base | {FormatadorKz.Formatar(item.ValorTotalVendido)}");
                break;

            case "Mensal" when Mensal is not null:
                sb.AppendLine($"Período: {Mensal.Mes:00}/{Mensal.Ano}");
                sb.AppendLine($"Receita: {FormatadorKz.Formatar(Mensal.Receita)}");
                sb.AppendLine($"Lucro estimado: {FormatadorKz.Formatar(Mensal.LucroEstimado)}");
                sb.AppendLine();
                sb.AppendLine("PRODUTOS MAIS VENDIDOS");
                foreach (var item in Mensal.ProdutosMaisVendidos)
                    sb.AppendLine($"{item.ProdutoNome} | {item.QuantidadeUnidadeBase:N2} un.base | {FormatadorKz.Formatar(item.ValorTotalVendido)}");
                break;

            case "Stock" when Stock is not null:
                sb.AppendLine($"Em falta: {Stock.ProdutosEmFalta.Count}");
                sb.AppendLine($"Stock baixo: {Stock.ProdutosComStockBaixo.Count}");
                sb.AppendLine();
                sb.AppendLine("PRODUTOS EM FALTA");
                foreach (var item in Stock.ProdutosEmFalta)
                    sb.AppendLine($"{item.ProdutoNome} | {item.QuantidadeApresentacao:N2} {item.UnidadeStock} | mínimo {item.StockMinimoApresentacao:N2}");
                sb.AppendLine();
                sb.AppendLine("PRODUTOS COM STOCK BAIXO");
                foreach (var item in Stock.ProdutosComStockBaixo)
                    sb.AppendLine($"{item.ProdutoNome} | atual {item.EstoqueAtual:N2} | mínimo {item.StockMinimo:N2}");
                break;
        }

        return sb.ToString();
    }
}