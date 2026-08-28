using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using KiVenda.Core.Enums;

namespace KiVenda.Desktop.Converters;

public sealed class MetodoPagamentoParaTextoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        MetodoPagamento.Dinheiro => "Dinheiro",
        MetodoPagamento.Multicaixa => "Multicaixa",
        MetodoPagamento.Tpa => "TPA",
        _ => "—"
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TipoMovimentoCaixaParaTextoConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        TipoMovimentoCaixa.Suprimento => "Suprimento",
        TipoMovimentoCaixa.Sangria => "Sangria",
        TipoMovimentoCaixa.Venda => "Venda",
        _ => "—"
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Suprimento e Venda são entradas (verde); Sangria é saída (vermelho) — mesma lógica de <c>MovimentoCaixa.EhEntrada</c> no Core.</summary>
public sealed class TipoMovimentoCaixaParaCorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        TipoMovimentoCaixa.Sangria => new SolidColorBrush(Color.Parse("#DC2626")),
        _ => new SolidColorBrush(Color.Parse("#16A34A"))
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Prefixa "+ " para entradas e "- " para saídas, junto com o valor em Kz.</summary>
public sealed class TipoMovimentoCaixaParaSinalConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoMovimentoCaixa.Sangria ? "-" : "+";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
