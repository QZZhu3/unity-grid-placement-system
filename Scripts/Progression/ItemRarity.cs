/// <summary>
/// Rarity tier for a placeable decoration item.
/// Rarity affects drop weight in reward pools and visual presentation in the UI.
/// </summary>
public enum ItemRarity
{
    /// <summary>Freely available, high drop weight.</summary>
    Common    = 0,

    /// <summary>Moderately available, medium drop weight.</summary>
    Uncommon  = 1,

    /// <summary>Rarely available, low drop weight.</summary>
    Rare      = 2,

    /// <summary>Only available during specific seasonal events.</summary>
    Seasonal  = 3,

    /// <summary>One-of-a-kind or story-gated items.</summary>
    Unique    = 4
}
