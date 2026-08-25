using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using KiVenda.Core.Enums;

namespace KiVenda.Desktop.Converters;

public sealed class EstadoStockParaTextoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        EstadoStock.EmStock => "Em Stock",
        EstadoStock.StockBaixo => "Baixo Stock",
        EstadoStock.SemStock => "Sem Stock",
        _ => "—"
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class EstadoStockParaCorFundoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        EstadoStock.EmStock => new SolidColorBrush(Color.Parse("#DCFCE7")),
        EstadoStock.StockBaixo => new SolidColorBrush(Color.Parse("#FEE2E2")),
        EstadoStock.SemStock => new SolidColorBrush(Color.Parse("#F1F5F9")),
        _ => new SolidColorBrush(Color.Parse("#F1F5F9"))
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class EstadoStockParaCorTextoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        EstadoStock.EmStock => new SolidColorBrush(Color.Parse("#16A34A")),
        EstadoStock.StockBaixo => new SolidColorBrush(Color.Parse("#DC2626")),
        EstadoStock.SemStock => new SolidColorBrush(Color.Parse("#6B7280")),
        _ => new SolidColorBrush(Color.Parse("#6B7280"))
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
