using System.Globalization;

namespace LeltarKezelo.Helpers;

/// <summary>
/// Felhasználói számbevitel kezelése: tizedesvesszőt és -pontot is elfogad,
/// a szóközöket (ezres tagolás, nem törhető szóköz) figyelmen kívül hagyja.
/// </summary>
public static class DecimalParser
{
    public static bool TryParse(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray())
            .Replace(',', '.');

        return decimal.TryParse(
            normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    /// <summary>Szerkesztéshez formázott érték (felesleges tizedesjegyek nélkül).</summary>
    public static string Format(decimal value) =>
        value.ToString("0.####", CultureInfo.CurrentCulture);
}
