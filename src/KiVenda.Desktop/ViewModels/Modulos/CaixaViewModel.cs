using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KiVenda.Application.Caixa;
using KiVenda.Core.Exceptions;
using KiVenda.Desktop.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Fluxo de caixa (Secção 4.5): Abrir Caixa → vendas/entradas/saídas →
/// Fechar Caixa. No fecho, mostra a divergência apurada — quanto
/// dinheiro deveria existir versus o que foi informado.
/// </summary>
public partial class CaixaViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    [ObservableProperty]
    private bool _caixaAberto;

    [ObservableProperty]
    private bool _aCarregar;

    [ObservableProperty]
    private string? _mensagemErro;

    [ObservableProperty]
    private string _saldoAtualTexto = "0 Kz";

    [ObservableProperty]
    private string _totalEntradasTexto = "0 Kz";

    [ObservableProperty]
    private string _totalSaidasTexto = "0 Kz";

    public ObservableCollection<MovimentoCaixaDto> Movimentos { get; } = new();

    // ---- Últimas Movimentações: busca instantânea + paginação (30/página) ----
    private const int TamanhoPagina = 30;

    [ObservableProperty]
    private string? _textoBusca;

    [ObservableProperty]
    private int _paginaAtual = 1;

    public ObservableCollection<MovimentoCaixaDto> MovimentosPaginados { get; } = new();

    public string IntervaloRegistosTexto
    {
        get
        {
            var total = MovimentosFiltrados().Count;
            if (total == 0) return "Nenhuma transação encontrada";
            var inicio = (PaginaAtual - 1) * TamanhoPagina + 1;
            var fim = Math.Min(PaginaAtual * TamanhoPagina, total);
            return $"Exibindo {inicio}–{fim} de {total} transações";
        }
    }

    public bool PodeIrParaAnterior => PaginaAtual > 1;

    public bool PodeIrParaProxima => PaginaAtual * TamanhoPagina < MovimentosFiltrados().Count;

    public string SessaoInfoTexto => "Sessão em curso"; // TODO: substituir por dados reais de abertura (ver nota no chat)

    public ObservableCollection<ResumoMetodoDto> ResumoPorMetodo { get; } = new();

    partial void OnTextoBuscaChanged(string? value)
    {
        PaginaAtual = 1;
        AtualizarMovimentosPaginados();
    }

    private List<MovimentoCaixaDto> MovimentosFiltrados()
    {
        if (string.IsNullOrWhiteSpace(TextoBusca))
            return Movimentos.ToList();

        var termo = TextoBusca.Trim();
        return Movimentos.Where(m =>
                (m.Descricao?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false) ||
                m.Tipo.ToString().Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                m.Valor.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(termo, StringComparison.OrdinalIgnoreCase)
                // TODO: incluir Horário e Operador aqui assim que estes campos existirem em MovimentoCaixaDto
            )
            .ToList();
    }

    private void AtualizarMovimentosPaginados()
    {
        MovimentosPaginados.Clear();
        foreach (var movimento in MovimentosFiltrados()
                     .Skip((PaginaAtual - 1) * TamanhoPagina)
                     .Take(TamanhoPagina))
        {
            MovimentosPaginados.Add(movimento);
        }

        OnPropertyChanged(nameof(IntervaloRegistosTexto));
        OnPropertyChanged(nameof(PodeIrParaAnterior));
        OnPropertyChanged(nameof(PodeIrParaProxima));
    }

    [RelayCommand]
    private void PaginaAnterior()
    {
        if (!PodeIrParaAnterior) return;
        PaginaAtual--;
        AtualizarMovimentosPaginados();
    }

    [RelayCommand]
    private void ProximaPagina()
    {
        if (!PodeIrParaProxima) return;
        PaginaAtual++;
        AtualizarMovimentosPaginados();
    }

    [RelayCommand]
    private void EditarMovimento(MovimentoCaixaDto movimento)
    {
        // TODO: abrir o formulário/diálogo de edição do movimento selecionado.
    }

    [RelayCommand]
    private void ExportarPdf()
    {
        // Sem ação por agora, conforme pedido.
    }

    [RelayCommand]
    private void ExportarExcel()
    {
        // Sem ação por agora, conforme pedido.
    }

    // Abrir caixa
    [ObservableProperty]
    private bool _formularioAbrirAberto;

    [ObservableProperty]
    private string _saldoInicialInput = string.Empty;

    // Suprimento / Sangria
    [ObservableProperty]
    private bool _formularioSuprimentoAberto;

    [ObservableProperty]
    private string _valorSuprimentoInput = string.Empty;

    [ObservableProperty]
    private bool _formularioSangriaAberto;

    [ObservableProperty]
    private string _valorSangriaInput = string.Empty;

    // Fechar caixa
    [ObservableProperty]
    private bool _formularioFecharAberto;

    [ObservableProperty]
    private string _saldoInformadoInput = string.Empty;

    [ObservableProperty]
    private string? _resultadoFecho;

    [ObservableProperty]
    private bool _aProcessar;

    public CaixaViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
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
            var useCase = scope.ServiceProvider.GetRequiredService<ConsultarMovimentacoesCaixaUseCase>();
            var resumo = await useCase.ExecutarAsync(new ConsultarMovimentacoesCaixaQuery());

            CaixaAberto = true;
            SaldoAtualTexto = FormatadorKz.Formatar(resumo.SaldoCalculado);
            TotalEntradasTexto = FormatadorKz.Formatar(resumo.TotalEntradas);
            TotalSaidasTexto = FormatadorKz.Formatar(resumo.TotalSaidas);

            Movimentos.Clear();
            foreach (var movimento in resumo.Movimentos)
            {
                Movimentos.Add(movimento);
            }

            PaginaAtual = 1;
            AtualizarMovimentosPaginados();
        }
        catch (DomainException)
        {
            // "Sessão de caixa não encontrada" — nenhuma sessão aberta.
            CaixaAberto = false;
            Movimentos.Clear();
        }
        finally
        {
            ACarregar = false;
        }
    }

    [RelayCommand]
    private void AbrirFormularioAbrir() => FormularioAbrirAberto = true;

    [RelayCommand]
    private async Task ConfirmarAbrirCaixaAsync()
    {
        if (!decimal.TryParse(SaldoInicialInput, out var saldoInicial) || saldoInicial < 0)
        {
            MensagemErro = "Saldo inicial inválido.";
            return;
        }

        AProcessar = true;
        MensagemErro = null;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<AbrirCaixaUseCase>();
            await useCase.ExecutarAsync(new AbrirCaixaCommand(saldoInicial));

            FormularioAbrirAberto = false;
            SaldoInicialInput = string.Empty;
            await CarregarAsync();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AProcessar = false;
        }
    }

    [RelayCommand]
    private void AbrirFormularioSuprimento() => FormularioSuprimentoAberto = true;

    [RelayCommand]
    private async Task ConfirmarSuprimentoAsync()
    {
        if (!decimal.TryParse(ValorSuprimentoInput, out var valor) || valor <= 0)
        {
            MensagemErro = "Valor inválido.";
            return;
        }

        AProcessar = true;
        MensagemErro = null;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RegistarSuprimentoUseCase>();
            await useCase.ExecutarAsync(new RegistarSuprimentoCommand(valor, "Reforço de troco"));

            FormularioSuprimentoAberto = false;
            ValorSuprimentoInput = string.Empty;
            await CarregarAsync();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AProcessar = false;
        }
    }

    [RelayCommand]
    private void AbrirFormularioSangria() => FormularioSangriaAberto = true;

    [RelayCommand]
    private async Task ConfirmarSangriaAsync()
    {
        if (!decimal.TryParse(ValorSangriaInput, out var valor) || valor <= 0)
        {
            MensagemErro = "Valor inválido.";
            return;
        }

        AProcessar = true;
        MensagemErro = null;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<RegistarSangriaUseCase>();
            await useCase.ExecutarAsync(new RegistarSangriaCommand(valor, "Recolha de excesso em caixa"));

            FormularioSangriaAberto = false;
            ValorSangriaInput = string.Empty;
            await CarregarAsync();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AProcessar = false;
        }
    }

    [RelayCommand]
    private void AbrirFormularioFechar() => FormularioFecharAberto = true;

    [RelayCommand]
    private async Task ConfirmarFecharCaixaAsync()
    {
        if (!decimal.TryParse(SaldoInformadoInput, out var saldoInformado) || saldoInformado < 0)
        {
            MensagemErro = "Saldo informado inválido.";
            return;
        }

        AProcessar = true;
        MensagemErro = null;
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var useCase = scope.ServiceProvider.GetRequiredService<FecharCaixaUseCase>();
            var resultado = await useCase.ExecutarAsync(new FecharCaixaCommand(saldoInformado));

            var sinal = resultado.Divergencia >= 0 ? "sobra" : "falta";
            ResultadoFecho = $"Caixa fechado. Esperado: {FormatadorKz.Formatar(resultado.SaldoCalculado)} · " +
                              $"Informado: {FormatadorKz.Formatar(resultado.SaldoInformado)} · " +
                              $"Divergência: {FormatadorKz.Formatar(Math.Abs(resultado.Divergencia))} ({sinal})";

            FormularioFecharAberto = false;
            SaldoInformadoInput = string.Empty;
            await CarregarAsync();
        }
        catch (DomainException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            AProcessar = false;
        }
    }
}

/// <summary>
/// Linha de resumo do cartão "Por Método" (ex.: Dinheiro · 85k).
/// Preenchido a partir do agrupamento de movimentos por método de pagamento
/// assim que MovimentoCaixaDto expuser esse campo — ver nota no chat.
/// </summary>
public record ResumoMetodoDto(string Nome, string ValorTexto);
