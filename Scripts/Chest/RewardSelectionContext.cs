using System.Collections.Generic;

/// <summary>
/// Immutable context object passed to <see cref="RewardDrawService"/> for a single draw.
///
/// Encapsulates all inputs that influence reward selection:
/// the chest being opened, optional season filter, and future extension points
/// such as recency history and owned items for duplicate protection.
///
/// Keeping all draw inputs in one object makes the draw pipeline testable
/// and decoupled from any specific caller.
/// </summary>
public class RewardSelectionContext
{
    // -- Required --------------------------------------------------------------

    /// <summary>The chest definition driving this draw.</summary>
    public ChestDefinition Chest { get; }

    // -- Optional filters ------------------------------------------------------

    /// <summary>
    /// Active season tag. When set, only items matching this season (or non-seasonal
    /// items if <see cref="ChestDefinition.AllowNonSeasonalItems"/> is true) are eligible.
    /// Null means no seasonal filter is applied.
    /// </summary>
    public SeasonTag ActiveSeason { get; }

    // -- Future extension points -----------------------------------------------
    // These are wired up now so the pipeline can check them without a rewrite later.

    /// <summary>
    /// Item IDs the player has received recently. Used by recency decay logic.
    /// Empty by default until recency tracking is implemented.
    /// </summary>
    public IReadOnlyList<string> RecentItemIds { get; }

    /// <summary>
    /// Item IDs the player currently owns. Used by duplicate protection logic.
    /// Empty by default until duplicate protection is implemented.
    /// </summary>
    public IReadOnlyList<string> OwnedItemIds { get; }

    // -- Constructor -----------------------------------------------------------

    public RewardSelectionContext(
        ChestDefinition        chest,
        SeasonTag              activeSeason   = null,
        IReadOnlyList<string>  recentItemIds  = null,
        IReadOnlyList<string>  ownedItemIds   = null)
    {
        Chest         = chest;
        ActiveSeason  = activeSeason;
        RecentItemIds = recentItemIds  ?? System.Array.Empty<string>();
        OwnedItemIds  = ownedItemIds   ?? System.Array.Empty<string>();
    }
}
