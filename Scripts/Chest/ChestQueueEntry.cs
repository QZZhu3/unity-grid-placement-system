using System;

/// <summary>
/// Represents a single entry in the chest queue.
///
/// Wraps a <see cref="ChestDefinition"/> with metadata to support
/// future seasonal events, analytics, and pity systems.
/// </summary>
[Serializable]
public class ChestQueueEntry
{
    // ── Core ──────────────────────────────────────────────────────────────────

    /// <summary>The chest type to be opened.</summary>
    public ChestDefinition ChestDefinition;

    /// <summary>UTC Unix timestamp of when this chest was earned.</summary>
    public long EarnedAtUtc;

    // ── Metadata (future extensibility) ──────────────────────────────────────

    /// <summary>
    /// Optional tag describing how this chest was earned.
    /// Examples: "task_placement", "event_reward", "daily_login", "milestone"
    /// </summary>
    public string SourceTag;

    /// <summary>
    /// Optional event or season ID this chest is associated with.
    /// Used for seasonal chest filtering and analytics.
    /// </summary>
    public string EventId;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ChestQueueEntry(ChestDefinition definition, string sourceTag = "", string eventId = "")
    {
        ChestDefinition = definition;
        EarnedAtUtc     = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SourceTag       = sourceTag;
        EventId         = eventId;
    }
}
