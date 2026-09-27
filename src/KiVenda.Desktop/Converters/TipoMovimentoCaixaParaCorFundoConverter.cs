using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace KiVenda.Desktop.Converters;

/// <summary>
/// Converte o tipo de movimento de caixa (Entrada/Saída) na cor de fundo
/// usada nas etiquetas (pills) da coluna TIPO em "Últimas Movimentações".
/// Não depende diretamente do tipo enum para evitar acoplamento — usa o
/// texto do valor (ToString) tal como os restantes conversores de Tipo.
/// </summary>
public class TipoMovimentoCaixaParaCorFundoConverter : IValueConverter
{
    private static readonly IBrush FundoEntrada = new SolidColorBrush(Color.Parse("#E3F5E9"));
    private static readonly IBrush FundoSaida = new SolidColorBrush(Color.Parse("#FBE7E7"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var texto = value?.ToString() ?? string.Empty;
        return texto.Contains("Sa", StringComparison.OrdinalIgnoreCase)
            ? FundoSaida
            : FundoEntrada;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
