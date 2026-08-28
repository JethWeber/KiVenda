using System.Collections.ObjectModel;
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
