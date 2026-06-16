using System.Text.Json;
using System.Text.Json.Serialization;

namespace TarotTraining;

/// <summary>A single tarot card and everything we know about it (loaded from cards.json).</summary>
public sealed record Card(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("suit")] string Suit,
    [property: JsonPropertyName("arcana")] string Arcana,
    [property: JsonPropertyName("keywords")] string Keywords,
    [property: JsonPropertyName("brief")] string Brief,
    [property: JsonPropertyName("description")] string Description);

/// <summary>The whole deck plus the per-suit overview text — the single source of truth.</summary>
public sealed record TarotDeck(
    [property: JsonPropertyName("suits")] IReadOnlyDictionary<string, string> Suits,
    [property: JsonPropertyName("cards")] IReadOnlyList<Card> Cards)
{
    /// <summary>The selectable suit filters, in the order the original UI listed them.</summary>
    public static readonly IReadOnlyList<string> SuitOrder =
        ["Major Arcana", "Cups", "Pentacles", "Swords", "Wands"];

    public Card? Find(string name) => Cards.FirstOrDefault(c => c.Name == name);

    /// <summary>The cards belonging to a suit, or the whole deck for "All".</summary>
    public IReadOnlyList<Card> CardsFor(string suit) =>
        suit == "All" ? Cards : Cards.Where(c => c.Suit == suit).ToList();

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Loads cards.json from the application directory.</summary>
    public static TarotDeck Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "cards.json");
        using FileStream stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<TarotDeck>(stream, Options)
               ?? throw new InvalidDataException($"Could not parse {path}.");
    }
}
