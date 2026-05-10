using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Filters a candidate item list down to eligible items for a specific draw context.
///
/// Each filter step is independent and can be toggled via the chest definition.
/// New filter steps (pity, recency decay, duplicate protection) can be added
/// here without modifying any other system.
///
/// This class is a pure utility — it holds no state and is not a MonoBehaviour.
/// </summary>
public static class RewardFilterPipeline
{
    /// <summary>
    /// Runs all applicable filter steps and returns the eligible item list.
    ///
    /// Filter order:
    ///   1. Seasonal filter  — remove items that don't match the active season
    ///   2. Recency decay    — (stub) deprioritise recently received items
    ///   3. Duplicate guard  — (stub) deprioritise already-owned items
    /// </summary>
    /// <param name="candidates">All unlocked items from <see cref="ItemRewardPool"/>.</param>
    /// <param name="context">Draw context carrying chest config and player state.</param>
    /// <returns>Filtered list of eligible items. Never null; may be empty.</returns>
    public static List<PlaceableItem> Filter(
        List<PlaceableItem>   candidates,
        RewardSelectionContext context)
    {
        if (candidates == null || candidates.Count == 0)
            return new List<PlaceableItem>();

        List<PlaceableItem> result = new List<PlaceableItem>(candidates);

        // ── Step 1: Seasonal filter ───────────────────────────────────────────
        result = ApplySeasonalFilter(result, context);

        // ── Step 2: Recency decay (stub — no-op until implemented) ────────────
        if (context.Chest.UseRecencyDecay)
            result = ApplyRecencyDecay(result, context);

        // ── Step 3: Duplicate protection (stub — no-op until implemented) ─────
        if (context.Chest.UseDuplicateProtection)
            result = ApplyDuplicateProtection(result, context);

        // Safety fallback: if all items were filtered out, return full candidate list
        if (result.Count == 0)
        {
            Debug.LogWarning(
                $"[RewardFilterPipeline] All items filtered out for chest '{context.Chest.Id}'. " +
                $"Falling back to unfiltered candidate list.");
            return new List<PlaceableItem>(candidates);
        }

        return result;
    }

    // ── Private filter steps ──────────────────────────────────────────────────

    private static List<PlaceableItem> ApplySeasonalFilter(
        List<PlaceableItem>   items,
        RewardSelectionContext context)
    {
        // No active season and no chest-level season restriction → pass all through
        if (context.ActiveSeason == null && (context.Chest.AllowedSeasonTags == null
            || context.Chest.AllowedSeasonTags.Length == 0))
            return items;

        List<PlaceableItem> filtered = new List<PlaceableItem>();

        foreach (PlaceableItem item in items)
        {
            bool hasNoSeasonTag = item.SeasonalTags == null || item.SeasonalTags.Length == 0;

            // Non-seasonal items: include if the chest allows them
            if (hasNoSeasonTag)
            {
                if (context.Chest.AllowNonSeasonalItems)
                    filtered.Add(item);
                continue;
            }

            // Chest has explicit allowed seasons → item must match one of them
            if (context.Chest.AllowedSeasonTags != null && context.Chest.AllowedSeasonTags.Length > 0)
            {
                bool matchesChestSeason = false;
                foreach (SeasonTag allowed in context.Chest.AllowedSeasonTags)
                {
                    if (item.HasSeasonTag(allowed))
                    {
                        matchesChestSeason = true;
                        break;
                    }
                }
                if (!matchesChestSeason) continue;
            }

            // Active season filter: item must match the active season
            if (context.ActiveSeason != null && !item.HasSeasonTag(context.ActiveSeason))
                continue;

            filtered.Add(item);
        }

        return filtered;
    }

    /// <summary>
    /// Stub: recency decay will reduce the weight of recently received items.
    /// Currently returns the list unchanged.
    /// </summary>
    private static List<PlaceableItem> ApplyRecencyDecay(
        List<PlaceableItem>   items,
        RewardSelectionContext context)
    {
        // TODO: Implement recency decay.
        // Suggested approach: build a weighted list where items in
        // context.RecentItemIds receive a reduced weight multiplier (e.g. 0.25×).
        return items;
    }

    /// <summary>
    /// Stub: duplicate protection will reduce the weight of already-owned items.
    /// Currently returns the list unchanged.
    /// </summary>
    private static List<PlaceableItem> ApplyDuplicateProtection(
        List<PlaceableItem>   items,
        RewardSelectionContext context)
    {
        // TODO: Implement duplicate protection.
        // Suggested approach: remove or down-weight items whose IDs appear in
        // context.OwnedItemIds, with a configurable minimum weight floor.
        return items;
    }
}
