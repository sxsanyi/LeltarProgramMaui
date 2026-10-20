using LeltarKezelo.Helpers;
using LeltarKezelo.Models;

namespace LeltarKezelo.Data;

/// <summary>
/// Determinisztikus demóadat-generátor (fix seed), egyedi EAN-13 vonalkódokkal.
/// </summary>
public static class DemoDataGenerator
{
    private static readonly string[] Country = ["magyar", "olasz", "cseh", "orosz", "lengyel"];

    private static readonly string[] Adjectives =
    [
        "fehér", "friss", "házi", "prémium", "light",
        "teljes kiőrlésű", "natúr", "csípős", "gluténmentes"
    ];

    private static readonly string[] Nouns =
    [
        "alma", "körte", "barack", "kenyér", "tej", "vaj", "liszt", "rizs", "cukor", "csokoládé", "keksz", "sör", "cukor"
    ];

    private static readonly string[] Sizes = ["0,5 l", "1 l", "100 g", "250 g", "500 g", "1 kg"];

    public static List<Product> Create(int count)
    {
        var random = new Random(1980);
        var products = new List<Product>(count);

        for (var i = 0; i < count; i++)
        {
            // Index alapú kombináció → 5 * 10 * 20 * 7 = 7000 egyedi név
            var name = $"{Country[i % Country.Length]} " +
                       $"{Adjectives[i / Country.Length % Adjectives.Length]} " +
                       $"{Nouns[i / (Country.Length * Adjectives.Length) % Nouns.Length]} " +
                       $"{Sizes[i / (Country.Length * Adjectives.Length * Nouns.Length) % Sizes.Length]}";

            var quantity = random.NextDouble() < 0.12 ? 0 : random.Next(1, 250);

            products.Add(new Product
            {
                Name = name,
                Barcode = CreateEan13(i),
                Price = random.Next(15, 600) * 10m - 10m, // pl. 1 290 Ft
                Quantity = quantity,
                SearchName = TextNormalizer.ForSearch(name)
            });
        }

        return products;
    }

    /// <summary>érvényes magyarországi EAN-13 kód gyártása</summary>
    private static string CreateEan13(int index)
    {
        var body = "599" + (100_000_000 + index).ToString("D9");
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = body[i] - '0';
            sum += i % 2 == 0 ? digit : digit * 3;
        }

        var check = (10 - sum % 10) % 10;
        return body + check;
    }
}
