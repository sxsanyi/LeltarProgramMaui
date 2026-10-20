using System.Globalization;
using System.Text;

namespace LeltarKezelo.Helpers;

public static class TextNormalizer
{
    /// <summary>
    /// Keresési kulcs: trimmelt, kisbetűs, ékezetek nélküli szöveg.
    /// Pl. "Teljes Kiőrlésű KENYÉR" → "teljes kiorlesu kenyer".
    /// </summary>
    public static string ForSearch(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var decomposed = text.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }

    /// <summary>LIKE mintában speciális karakterek (\, %, _) escape-elése.</summary>
    public static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
