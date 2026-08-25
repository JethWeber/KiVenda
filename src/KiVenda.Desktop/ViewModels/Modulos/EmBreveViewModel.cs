namespace KiVenda.Desktop.ViewModels.Modulos;

/// <summary>
/// Placeholder para os módulos que aparecem no menu (fiel aos mockups,
/// Secção 8: 10 módulos) mas cuja implementação pertence a fases
/// seguintes do plano — Vendas/Caixa (Fase 7), Relatórios (Fase 9),
/// Configurações (Fase 11). Evita esconder itens do menu que os
/// mockups mostram, sem fingir que já estão prontos.
/// </summary>
public sealed class EmBreveViewModel : ViewModelBase
{
    public string NomeModulo { get; }

    public string Mensagem { get; }

    public EmBreveViewModel(string nomeModulo, string faseResponsavel)
    {
        NomeModulo = nomeModulo;
        Mensagem = $"O módulo \"{nomeModulo}\" será implementado na {faseResponsavel} do plano de implementação.";
    }
}
