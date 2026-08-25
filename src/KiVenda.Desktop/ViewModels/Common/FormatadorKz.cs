using System.Globalization;

namespace KiVenda.Desktop.ViewModels.Common;

/// <summary>
/// Formata valores em Kz de forma consistente em toda a UI, sem nunca
/// depender da cultura do sistema operativo — a mesma lição da Fase 4
/// (<c>ServicoImpressaoTexto</c>): a cultura "pt-AO" varia de máquina
/// para máquina, por isso construímos sempre a formatação manualmente.
/// </summary>
public static class FormatadorKz
{
    private static readonly CultureInfo Cultura = CriarCultura();

    public static string Formatar(decimal valor, int casasDecimais = 0)
    {
        var formato = casasDecimais == 0 ? "N0" : "N" + casasDecimais;
        return $"{valor.ToString(formato, Cultura)} Kz";
    }

    private static CultureInfo CriarCultura()
    {
        var cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return cultura;
    }
}
