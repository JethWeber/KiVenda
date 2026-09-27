using System.Globalization;
using Avalonia.Data.Converters;

namespace KiVenda.Desktop.Converters;

/// <summary>
/// Extrai as iniciais de um nome (primeira letra do primeiro e do
/// último nome) para desenhar avatares circulares sem depender de
/// fotografias — usado nas listagens de Cadastros.
/// </summary>
public sealed class IniciaisConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string nome || string.IsNullOrWhiteSpace(nome))
        {
            return "—";
        }

        var partes = nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0)
        {
            return "—";
        }

        var iniciais = partes.Length == 1
            ? partes[0][..1]
            : partes[0][..1] + partes[^1][..1];

        return iniciais.ToUpperInvariant();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
