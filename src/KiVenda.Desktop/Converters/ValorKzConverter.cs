using System.Globalization;
using Avalonia.Data.Converters;
using KiVenda.Desktop.ViewModels.Common;

namespace KiVenda.Desktop.Converters;

/// <summary>
/// Converte um <see cref="decimal"/> para texto em Kz usando
/// <see cref="FormatadorKz"/> — nunca a formatação por omissão do
/// binding, pela mesma razão documentada lá (não confiar na cultura do
/// sistema). Parâmetro opcional: número de casas decimais (ex.:
/// <c>ConverterParameter=2</c> para valores unitários).
/// </summary>
public sealed class ValorKzConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not decimal valor)
        {
            return "—";
        }

        var casasDecimais = parameter is string texto && int.TryParse(texto, out var n) ? n : 0;
        return FormatadorKz.Formatar(valor, casasDecimais);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
