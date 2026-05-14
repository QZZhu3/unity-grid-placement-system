using System.Collections.Generic;

/// <summary>
/// Contract for any system that selects and scores task definitions
/// for presentation to the player.
///
/// Architecture role:
///   TaskGenerator calls this interface to decide WHICH task to present next.
///   The current implementation is a simple weighted-random selector.
///   A future ML/AI system replaces only this implementation -- TaskGenerator,
///   ActivityManager, and all UI remain unchanged.
///
/// Integration guide for a future recommendation system:
///   1. Create a new class implementing ITaskRecommendationProvider.
///   2. Assign it to TaskGenerator.recommendationProvider in the Inspector
///      (or via dependency injection).
///   3. The new provider receives the full candidate pool, player telemetry
///      snapshot, and current player context, and returns a scored result.
///   4. No other system needs to change.
///
/// Design constraints:
///   - Must be synchronous (Unity main thread).
///   - Must never return null from SelectNext (return empty result instead).
///   - Must never modify TaskDefinition assets.
///   - Must never interact with UI directly.
/// </summary>
public interface ITaskRecommendationProvider
{
    /// <summary>
    /// Select the next task definition to present to the player.
    /// </summary>
    /// <param name="candidates">All available task definitions to choose from.</param>
    /// <param name="context">Current player state and telemetry snapshot.</param>
    /// <returns>
    /// A <see cref="RecommendationResult"/> describing which definition was selected
    /// and why. Never null -- return <see cref="RecommendationResult.Empty"/> if
    /// no suitable candidate is found.
    /// </returns>
    RecommendationResult SelectNext(
        IReadOnlyList<ActivityDefinition> candidates,
        RecommendationContext context);
}

/// <summary>
/// Contextual data passed to <see cref="ITaskRecommendationProvider.SelectNext"/>.
///
/// Contains everything a recommendation system needs to make an informed decision
/// without coupling it to specific manager types.
/// </summary>
public class RecommendationContext
{
    /// <summary>Player's current level.</summary>
    public int PlayerLevel { get; set; }

    /// <summary>Player's current XP total.</summary>
    public float PlayerXp { get; set; }

    /// <summary>
    /// Snapshot of recent player behaviour events.
    /// Provided by <see cref="PlayerTelemetryManager.GetRecentHistory"/>.
    /// </summary>
    public IReadOnlyList<TelemetryEvent> RecentEvents { get; set; }

    /// <summary>
    /// The RuntimeId of the task the player just completed, skipped, or rerolled.
    /// Empty string if this is the first task of the session.
    /// </summary>
    public string PreviousTaskRuntimeId { get; set; } = "";

    /// <summary>
    /// The DefinitionId of the task the player just completed, skipped, or rerolled.
    /// Used to avoid immediately re-recommending the same task.
    /// </summary>
    public string PreviousDefinitionId { get; set; } = "";

    /// <summary>
    /// How many tasks have been completed in this session.
    /// </summary>
    public int SessionTasksCompleted { get; set; }

    /// <summary>
    /// How many tasks have been skipped in this session.
    /// </summary>
    public int SessionTasksSkipped { get; set; }
}

/// <summary>
/// The output of <see cref="ITaskRecommendationProvider.SelectNext"/>.
/// </summary>
public class RecommendationResult
{
    /// <summary>The selected definition. Null only in <see cref="Empty"/>.</summary>
    public ActivityDefinition SelectedDefinition { get; set; }

    /// <summary>Confidence score (0-1). -1 = not scored (random selection).</summary>
    public float Score { get; set; } = -1f;

    /// <summary>Human-readable reason for this selection.</summary>
    public string Reason { get; set; } = "";

    /// <summary>Source tags describing how this recommendation was made.</summary>
    public string[] SourceTags { get; set; } = System.Array.Empty<string>();

    /// <summary>Difficulty modifier to apply to the generated TaskInstance.</summary>
    public float DifficultyModifier { get; set; } = 1f;

    /// <summary>Reward modifier to apply to the generated TaskInstance.</summary>
    public float RewardModifier { get; set; } = 1f;

    /// <summary>
    /// Returned when no suitable candidate is found.
    /// TaskGenerator treats this as "use fallback".
    /// </summary>
    public static readonly RecommendationResult Empty = new RecommendationResult
    {
        SelectedDefinition = null,
        Score              = -1f,
        Reason             = "no_candidates",
        SourceTags         = new[] { "empty" }
    };

    public bool IsEmpty => SelectedDefinition == null;
}
