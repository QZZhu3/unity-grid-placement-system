using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single rewarded item with its rarity context.
///
/// Serializable for save/log support. Stores item ID (not asset reference)
/// so it is safe to persist.
/// </summary>
[System.Serializable]
public class RewardResult
{
    /// <summary>The rewarded item asset.</summary>
    public PlaceableItem Item;

    /// <summary>The rarity tier at which this item was drawn.</summary>
    public ItemRarity DrawnRarity;

    /// <summary>Quantity of this item granted.</summary>
    public int Quantity;

    public RewardResult(PlaceableItem item, ItemRarity drawnRarity, int quantity = 1)
    {
        Item        = item;
        DrawnRarity = drawnRarity;
        Quantity    = quantity;
    }
}

/// <summary>
/// A collection of reward results from a single draw or chest opening.
///
/// Designed to be extended with future resource types (coins, scrap, currency)
/// without breaking existing code.
/// </summary>
[System.Serializable]
public class RewardBundle
{
    /// <summary>All item rewards in this bundle.</summary>
    public List<RewardResult> Items = new List<RewardResult>();

    // -- Future extension points -----------------------------------------------
    // public int CoinsGranted;
    // public int ScrapGranted;
    // public List<CurrencyReward> CurrencyRewards;

    /// <summary>True if this bundle contains at least one item.</summary>
    public bool HasItems => Items != null && Items.Count > 0;

    /// <summary>Total number of item rewards across all entries.</summary>
    public int TotalItemCount
    {
        get
        {
            int total = 0;
            foreach (RewardResult r in Items) total += r.Quantity;
            return total;
        }
    }
}

/// <summary>
/// The complete result of opening a single chest.
///
/// Carries the chest definition that was opened, the reward bundle,
/// and any metadata needed by the UI or analytics layer.
/// </summary>
[System.Serializable]
public class ChestOpenResult
{
    /// <summary>The chest definition that was opened.</summary>
    public ChestDefinition Chest;

    /// <summary>All rewards granted by opening this chest.</summary>
    public RewardBundle Bundle;

    /// <summary>UTC timestamp of when the chest was opened (Unix seconds).</summary>
    public long OpenedAtUtc;

    public ChestOpenResult(ChestDefinition chest, RewardBundle bundle)
    {
        Chest       = chest;
        Bundle      = bundle;
        OpenedAtUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>Convenience: true if the result contains at least one item reward.</summary>
    public bool HasRewards => Bundle != null && Bundle.HasItems;
}
