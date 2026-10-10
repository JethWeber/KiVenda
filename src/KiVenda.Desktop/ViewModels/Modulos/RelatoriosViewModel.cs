using System.Collections.ObjectModel;
using System.Globalization;
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
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PodeImprimir))] private string _relatorioSelecionado = "Diario";

    public bool MostrarDiario => RelatorioSelecionado == "Diario";
    public bool MostrarMensal => RelatorioSelecionado == "Mensal";
    public bool MostrarStock => RelatorioSelecionado == "Stock";
    public bool MostrarFiltrosData => !MostrarStock;
    public bool MostrarFiltroUtilizador => MostrarDiario;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PodeImprimir))] private bool _aCarregar;
    [ObservableProperty] private string? _mensagem;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PodeImprimir))] [NotifyPropertyChangedFor(nameof(DiarioSemVendas))] [NotifyPropertyChangedFor(nameof(DataReferenciaTexto))] private RelatorioDiarioDto? _diario;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PodeImprimir))] [NotifyPropertyChangedFor(nameof(MensalSemVendas))] [NotifyPropertyChangedFor(nameof(MesReferenciaTexto))] private RelatorioMensalDto? _mensal;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(PodeImprimir))] [NotifyPropertyChangedFor(nameof(SemProdutosEmFalta))] [NotifyPropertyChangedFor(nameof(SemStockBaixo))] private RelatorioStockDto? _stock;
    [ObservableProperty] private UtilizadorDto? _utilizadorSelecionado;

    public ObservableCollection<UtilizadorDto> Utilizadores { get; } = new();

    // ===================== Apoio à apresentação =====================

    private static readonly CultureInfo CulturaPt = new("pt-PT");

    /// <summary>Só imprime quando o relatório atual já está carregado.</summary>
    public bool PodeImprimir => !ACarregar && (RelatorioSelecionado switch
    {
        "Diario" => Diario is not null,
        "Mensal" => Mensal is not null,
        "Stock" => Stock is not null,
        _ => false
    });

    /// <summary>Ex.: "Sábado, 10 de outubro de 2026".</summary>
    public string DataReferenciaTexto => Diario is null
        ? string.Empty
        : Capitalizar(Diario.Data.ToString("dddd, dd 'de' MMMM 'de' yyyy", CulturaPt));

    /// <summary>Ex.: "Outubro de 2026".</summary>
    public string MesReferenciaTexto => Mensal is null
        ? string.Empty
        : Capitalizar(new DateTime(Mensal.Ano, Mensal.Mes, 1).ToString("MMMM 'de' yyyy", CulturaPt));

    // Estados vazios (a lista existe mas não tem linhas).
    public bool DiarioSemVendas => Diario is not null && !Diario.ProdutosVendidos.Any();
    public bool MensalSemVendas => Mensal is not null && !Mensal.ProdutosMaisVendidos.Any();
    public bool SemProdutosEmFalta => Stock is not null && !Stock.ProdutosEmFalta.Any();
    public bool SemStockBaixo => Stock is not null && !Stock.ProdutosComStockBaixo.Any();

    private static string Capitalizar(string texto) =>
        string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0], CulturaPt) + texto[1..];

    // ===================== Aviso (toast) de sucesso/erro: aparece 3 s e some sozinho =====================
    private CancellationTokenSource? _toastCts;

    [ObservableProperty] private bool _toastVisivel;
    [ObservableProperty] private bool _toastErro;
    [ObservableProperty] private string _toastTitulo = string.Empty;
    [ObservableProperty] private string _toastMensagem = string.Empty;

    private async void MostrarToast(bool erro, string mensagem)
    {
        _toastCts?.Cancel();
        var cts = _toastCts = new CancellationTokenSource();

        ToastErro = erro;
        ToastTitulo = erro ? "Erro" : "Sucesso";
        ToastMensagem = mensagem;
        ToastVisivel = true;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cts.Token);
            ToastVisivel = false;
        }
        catch (OperationCanceledException)
        {
            // Chegou outro aviso: ele reinicia a contagem.
        }
    }

    // Erros (carregar, imprimir) viram também aviso visual.
    partial void OnMensagemChanged(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            MostrarToast(erro: true, value);
    }

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
            MostrarToast(erro: false, "Relatório enviado para impressão.");
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