using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core reward selection engine.
///
/// Given a <see cref="RewardSelectionContext"/>, draws a <see cref="RewardBundle"/>
/// by querying <see cref="ItemRewardPool"/>, filtering through
/// <see cref="RewardFilterPipeline"/>, and performing weighted rarity selection.
///
/// This class is a pure service -- it holds no persistent state and is not a
/// MonoBehaviour. Instantiate it once and reuse it.
///
/// Data flow:
///   ChestDefinition
///   -> RewardSelectionContext
///   -> RewardDrawService.Draw()
///   -> ItemRewardPool (get all unlocked items)
///   -> RewardFilterPipeline (seasonal, recency, duplicate filters)
///   -> Weighted rarity roll
///   -> Random item selection per tier
///   -> RewardBundle
/// </summary>
public class RewardDrawService
{
    private readonly ItemRewardPool rewardPool;

    public RewardDrawService(ItemRewardPool rewardPool)
    {
        this.rewardPool = rewardPool;
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Draws a complete <see cref="RewardBundle"/> for the given context.
    /// The number of items drawn equals <see cref="ChestDefinition.RewardsPerChest"/>.
    /// </summary>
    public RewardBundle Draw(RewardSelectionContext context)
    {
        RewardBundle bundle = new RewardBundle();

        if (rewardPool == null)
        {
            Debug.LogError("[RewardDrawService] ItemRewardPool is null. Cannot draw rewards.");
            return bundle;
        }

        // Get all unlocked items from the pool (no season filter at pool level --
        // we apply our own filter pipeline below)
        List<PlaceableItem> candidates = rewardPool.GetEligibleItems();

        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning(
                $"[RewardDrawService] No eligible items in pool for chest '{context.Chest.Id}'.");
            return bundle;
        }

        // Run filter pipeline
        List<PlaceableItem> eligible = RewardFilterPipeline.Filter(candidates, context);

        // Build rarity weight table from chest definition (or fall back to pool defaults)
        Dictionary<ItemRarity, float> rarityWeights = BuildRarityWeights(context.Chest);

        // Draw the required number of rewards
        int count = Mathf.Max(1, context.Chest.RewardsPerChest);
        for (int i = 0; i < count; i++)
        {
            PlaceableItem item = DrawSingleItem(eligible, rarityWeights);
            if (item == null) continue;

            bundle.Items.Add(new RewardResult(item, item.Rarity, 1));
        }

        return bundle;
    }

    // -- Private helpers -------------------------------------------------------

    private PlaceableItem DrawSingleItem(
        List<PlaceableItem>          eligible,
        Dictionary<ItemRarity, float> rarityWeights)
    {
        if (eligible == null || eligible.Count == 0) return null;

        // Group items by rarity
        var byRarity = new Dictionary<ItemRarity, List<PlaceableItem>>();
        foreach (PlaceableItem item in eligible)
        {
            if (!byRarity.ContainsKey(item.Rarity))
                byRarity[item.Rarity] = new List<PlaceableItem>();
            byRarity[item.Rarity].Add(item);
        }

        // Weighted rarity roll (only include tiers that have eligible items)
        float total = 0f;
        var weightedTiers = new List<(ItemRarity rarity, float weight)>();

        foreach (var kvp in rarityWeights)
        {
            if (byRarity.ContainsKey(kvp.Key) && byRarity[kvp.Key].Count > 0)
            {
                weightedTiers.Add((kvp.Key, kvp.Value));
                total += kvp.Value;
            }
        }

        if (weightedTiers.Count == 0)
        {
            // Fallback: pick any random item
            return eligible[Random.Range(0, eligible.Count)];
        }

        float roll       = Random.Range(0f, total);
        float cumulative = 0f;
        ItemRarity chosenRarity = weightedTiers[weightedTiers.Count - 1].rarity;

        foreach ((ItemRarity rarity, float weight) in weightedTiers)
        {
            cumulative += weight;
            if (roll <= cumulative)
            {
                chosenRarity = rarity;
                break;
            }
        }

        // Pick a random item from the chosen rarity tier
        List<PlaceableItem> tier = byRarity[chosenRarity];
        return tier[Random.Range(0, tier.Count)];
    }

    private Dictionary<ItemRarity, float> BuildRarityWeights(ChestDefinition chest)
    {
        var weights = new Dictionary<ItemRarity, float>();

        if (chest.HasCustomRarityWeights)
        {
            foreach (ChestRarityWeight entry in chest.RarityWeights)
                weights[entry.Rarity] = entry.Weight;
        }
        else
        {
            // Default weights matching ItemRewardPool defaults
            weights[ItemRarity.Common]   = 60f;
            weights[ItemRarity.Uncommon] = 25f;
            weights[ItemRarity.Rare]     = 12f;
            weights[ItemRarity.Seasonal] =  3f;
        }

        return weights;
    }
}
