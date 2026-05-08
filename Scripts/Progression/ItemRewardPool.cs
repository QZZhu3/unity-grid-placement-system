using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the pool of <see cref="PlaceableItem"/> assets that can be awarded to the player.
///
/// Filtering is entirely data-driven via <see cref="UnlockManager.IsItemEligible"/>.
/// This class never performs its own unlock checks — it always delegates to
/// <see cref="UnlockManager"/>, which is the single source of truth.
///
/// Rarity weighting:
///   Each <see cref="ItemRarity"/> tier has a configurable drop weight.
///   The pool performs a two-step draw: first select a rarity tier by weight,
///   then select a random eligible item from that tier.
///   If no eligible items exist in the chosen tier, the draw falls back to
///   the next lower tier.
///
/// Seasonal filtering:
///   Pass a <see cref="SeasonTag"/> to <see cref="DrawItem(SeasonTag)"/> to restrict
///   the pool to seasonally tagged items. Pass null for a season-agnostic draw.
///
/// Usage:
///   1. Assign all game items to <see cref="allItems"/>.
///   2. Assign the <see cref="UnlockManager"/> reference.
///   3. Call <see cref="DrawItem()"/> or <see cref="DrawItem(SeasonTag)"/> to get a reward.
///   4. Call <see cref="DrawItems(int)"/> for multi-item reward chests.
/// </summary>
public class ItemRewardPool : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies")]
    [SerializeField] private UnlockManager unlockManager;

    [Header("Item Registry")]
    [Tooltip("Every PlaceableItem asset in the game. " +
             "The pool filters this list at draw time via UnlockManager.")]
    [SerializeField] private List<PlaceableItem> allItems = new List<PlaceableItem>();

    [Header("Rarity Weights")]
    [Tooltip("Drop weight for Common items. Higher = more likely to appear.")]
    [SerializeField] private float weightCommon    = 60f;
    [SerializeField] private float weightUncommon  = 25f;
    [SerializeField] private float weightRare      = 12f;
    [SerializeField] private float weightSeasonal  =  3f;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a single item from the eligible pool using rarity-weighted selection.
    /// Returns null if no eligible items exist.
    /// </summary>
    public PlaceableItem DrawItem(SeasonTag season = null)
    {
        List<PlaceableItem> eligible = GetEligibleItems(season);
        if (eligible.Count == 0) return null;

        return DrawFromEligible(eligible);
    }

    /// <summary>
    /// Draws multiple unique items from the eligible pool.
    /// If the pool has fewer items than requested, returns all available items.
    /// </summary>
    public List<PlaceableItem> DrawItems(int count, SeasonTag season = null)
    {
        List<PlaceableItem> eligible = GetEligibleItems(season);
        List<PlaceableItem> results  = new List<PlaceableItem>();

        // Shuffle a copy so we can draw without replacement
        List<PlaceableItem> pool = new List<PlaceableItem>(eligible);
        Shuffle(pool);

        int draws = Mathf.Min(count, pool.Count);
        for (int i = 0; i < draws; i++)
            results.Add(pool[i]);

        return results;
    }

    /// <summary>
    /// Returns all currently eligible items, optionally filtered by season.
    /// Delegates entirely to <see cref="UnlockManager"/> — no local unlock logic.
    /// </summary>
    public List<PlaceableItem> GetEligibleItems(SeasonTag season = null)
    {
        if (unlockManager == null)
        {
            Debug.LogWarning("[ItemRewardPool] UnlockManager reference is missing.");
            return new List<PlaceableItem>();
        }

        return season == null
            ? unlockManager.FilterEligible(allItems)
            : unlockManager.FilterEligibleForSeason(allItems, season);
    }

    /// <summary>
    /// Returns all eligible items of a specific rarity, optionally filtered by season.
    /// </summary>
    public List<PlaceableItem> GetEligibleItemsByRarity(ItemRarity rarity, SeasonTag season = null)
    {
        List<PlaceableItem> eligible = GetEligibleItems(season);
        List<PlaceableItem> result   = new List<PlaceableItem>();
        foreach (PlaceableItem item in eligible)
            if (item.Rarity == rarity) result.Add(item);
        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Two-step rarity-weighted draw:
    ///   1. Select a rarity tier by weight.
    ///   2. Pick a random item from that tier.
    ///   3. Fall back to lower tiers if the chosen tier is empty.
    /// </summary>
    private PlaceableItem DrawFromEligible(List<PlaceableItem> eligible)
    {
        // Build per-tier lists
        var byRarity = new Dictionary<ItemRarity, List<PlaceableItem>>();
        foreach (PlaceableItem item in eligible)
        {
            if (!byRarity.ContainsKey(item.Rarity))
                byRarity[item.Rarity] = new List<PlaceableItem>();
            byRarity[item.Rarity].Add(item);
        }

        // Weighted rarity roll
        ItemRarity chosen = RollRarity(byRarity);

        // Pick random item from chosen tier
        List<PlaceableItem> tier = byRarity[chosen];
        return tier[Random.Range(0, tier.Count)];
    }

    private ItemRarity RollRarity(Dictionary<ItemRarity, List<PlaceableItem>> byRarity)
    {
        // Build weight table only for tiers that have eligible items
        float total = 0f;
        var weights = new List<(ItemRarity rarity, float weight)>();

        void AddIfPresent(ItemRarity r, float w)
        {
            if (byRarity.ContainsKey(r) && byRarity[r].Count > 0)
            {
                weights.Add((r, w));
                total += w;
            }
        }

        AddIfPresent(ItemRarity.Seasonal,  weightSeasonal);
        AddIfPresent(ItemRarity.Rare,      weightRare);
        AddIfPresent(ItemRarity.Uncommon,  weightUncommon);
        AddIfPresent(ItemRarity.Common,    weightCommon);

        if (weights.Count == 0) return ItemRarity.Common; // fallback

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        foreach ((ItemRarity rarity, float weight) in weights)
        {
            cumulative += weight;
            if (roll <= cumulative) return rarity;
        }

        return weights[weights.Count - 1].rarity;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // ── Debug ─────────────────────────────────────────────────────────────────

    [ContextMenu("Debug: Print Eligible Item Count")]
    private void DebugPrintEligible()
    {
        List<PlaceableItem> eligible = GetEligibleItems();
        Debug.Log($"[ItemRewardPool] Eligible items: {eligible.Count} / {allItems.Count}");
        foreach (PlaceableItem item in eligible)
            Debug.Log($"  - {item.DisplayName} ({item.Rarity}) [{item.Category?.DisplayName ?? "No Category"}]");
    }
}
