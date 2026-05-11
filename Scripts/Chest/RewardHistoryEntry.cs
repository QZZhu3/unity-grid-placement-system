using System;

/// <summary>
/// Records a single item reward grant for analytics, pity, and duplicate balancing.
///
/// Not yet persisted to save — this is a runtime-only structure for now.
/// Hook it into <see cref="RewardDrawService"/> to enable recency decay,
/// pity counters, and duplicate protection in the future.
/// </summary>
[Serializable]
public class RewardHistoryEntry
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Stable string ID of the rewarded item (not asset reference, safe to persist).</summary>
    public string ItemId;

    /// <summary>Display name of the item at time of grant (for readable logs).</summary>
    public string ItemDisplayName;

    /// <summary>Rarity tier at which this item was drawn.</summary>
    public ItemRarity DrawnRarity;

    // ── Source ────────────────────────────────────────────────────────────────

    /// <summary>ID of the chest that produced this reward.</summary>
    public string SourceChestId;

    /// <summary>Optional source tag from the ChestQueueEntry (e.g. "task_placement").</summary>
    public string SourceTag;

    /// <summary>UTC Unix timestamp of when this reward was granted.</summary>
    public long GrantedAtUtc;

    // ── Pity / Recency hooks ──────────────────────────────────────────────────

    /// <summary>
    /// Number of draws since the last time this rarity was granted.
    /// Used for pity counter logic — increment on each draw, reset on match.
    /// </summary>
    public int RollsSinceLastRarity;

    /// <summary>
    /// Number of draws since this specific item was last granted.
    /// Used for recency decay — reduces weight of recently granted items.
    /// </summary>
    public int RollsSinceLastItem;

    // ── Constructor ───────────────────────────────────────────────────────────

    public RewardHistoryEntry(
        PlaceableItem item,
        ItemRarity drawnRarity,
        string sourceChestId,
        string sourceTag = "",
        int rollsSinceLastRarity = 0,
        int rollsSinceLastItem   = 0)
    {
        ItemId               = item != null ? item.ItemId : "unknown";
        ItemDisplayName      = item != null ? item.DisplayName : "Unknown";
        DrawnRarity          = drawnRarity;
        SourceChestId        = sourceChestId;
        SourceTag            = sourceTag;
        GrantedAtUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        RollsSinceLastRarity = rollsSinceLastRarity;
        RollsSinceLastItem   = rollsSinceLastItem;
    }
}
