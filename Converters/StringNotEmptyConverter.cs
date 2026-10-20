using System.Globalization;

namespace LeltarKezelo.Converters;

/// <summary>Igaz, ha a kötött érték nem üres szöveg (hibaüzenetek láthatóságához).</summary>
public sealed class StringNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && !string.IsNullOrWhiteSpace(text);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
