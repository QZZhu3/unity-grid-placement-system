using System;
using UnityEngine;

/// <summary>
/// A runtime-generated, mutable task object created from a static
/// <see cref="ActivityDefinition"/> template.
///
/// Architecture role:
///   ActivityDefinition (ScriptableObject, static data)
///   --> TaskGenerator creates TaskInstance at runtime
///   --> TaskInstance is what UI, ActivityManager, and Telemetry work with
///   --> TaskInstance is what gets saved/loaded, not the definition
///
/// Recommendation metadata fields are intentionally left as placeholders.
/// A future ITaskRecommendationProvider will populate them without requiring
/// any changes to this class.
/// </summary>
[Serializable]
public class TaskInstance
{
    // -- Identity --------------------------------------------------------------

    /// <summary>
    /// Unique runtime ID for this specific instance.
    /// Format: "task_{definitionId}_{utcUnixMs}"
    /// </summary>
    public string RuntimeId { get; private set; }

    /// <summary>
    /// The stable ID of the source <see cref="ActivityDefinition"/> asset.
    /// Used to re-link the definition on load without storing asset references.
    /// </summary>
    public string DefinitionId { get; private set; }

    /// <summary>
    /// The source definition. Not serialized -- resolved at load time via DefinitionId.
    /// </summary>
    [NonSerialized]
    public ActivityDefinition Definition;

    // -- Timestamps ------------------------------------------------------------

    /// <summary>UTC Unix timestamp (seconds) when this instance was generated.</summary>
    public long GeneratedAtUtc { get; private set; }

    // -- Completion state ------------------------------------------------------

    public bool IsCompleted { get; private set; }
    public long CompletedAtUtc { get; private set; }

    // -- Player interaction metadata -------------------------------------------

    /// <summary>How many times the player has skipped this task.</summary>
    public int SkipCount { get; private set; }

    /// <summary>How many times the player has rerolled away from this task.</summary>
    public int RerollCount { get; private set; }

    // -- Reward modifiers ------------------------------------------------------

    /// <summary>
    /// Multiplier applied on top of the definition's base XP reward.
    /// Default 1.0. A future recommendation system may boost this for
    /// high-priority or streak-aligned tasks.
    /// </summary>
    public float DifficultyModifier { get; private set; } = 1f;

    /// <summary>
    /// Multiplier applied on top of the definition's base chest progress ticks.
    /// Default 1.0.
    /// </summary>
    public float RewardModifier { get; private set; } = 1f;

    // -- Recommendation metadata placeholders ----------------------------------
    // These fields are intentionally empty until a recommendation provider
    // is integrated. They exist here so the save format and data model are
    // already correct when that system is added.

    /// <summary>
    /// Confidence score assigned by the recommendation system (0-1).
    /// Default -1 = not yet scored (random selection).
    /// </summary>
    public float RecommendationScore { get; private set; } = -1f;

    /// <summary>
    /// Human-readable reason why this task was recommended.
    /// Empty = not yet provided (random selection).
    /// </summary>
    public string RecommendationReason { get; private set; } = "";

    /// <summary>
    /// Tags describing the source of this recommendation.
    /// e.g. "random", "streak_aligned", "category_preferred", "ai_model_v1"
    /// </summary>
    public string[] SourceTags { get; private set; } = Array.Empty<string>();

    // -- Construction ----------------------------------------------------------

    /// <summary>
    /// Create a new TaskInstance from a definition.
    /// Called by <see cref="TaskGenerator"/> only.
    /// </summary>
    public TaskInstance(ActivityDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        Definition   = definition;
        DefinitionId = definition.Id;
        GeneratedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        RuntimeId    = $"task_{DefinitionId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }

    /// <summary>
    /// Restore a TaskInstance from saved data.
    /// The Definition reference must be resolved separately via DefinitionId.
    /// </summary>
    public static TaskInstance FromSaveData(TaskInstanceSaveData data)
    {
        var instance = new TaskInstance
        {
            RuntimeId            = data.runtimeId,
            DefinitionId         = data.definitionId,
            GeneratedAtUtc       = data.generatedAtUtc,
            IsCompleted          = data.isCompleted,
            CompletedAtUtc       = data.completedAtUtc,
            SkipCount            = data.skipCount,
            RerollCount          = data.rerollCount,
            DifficultyModifier   = data.difficultyModifier,
            RewardModifier       = data.rewardModifier,
            RecommendationScore  = data.recommendationScore,
            RecommendationReason = data.recommendationReason,
            SourceTags           = data.sourceTags ?? Array.Empty<string>()
        };
        return instance;
    }

    // Private constructor for FromSaveData
    private TaskInstance() { }

    // -- Mutation methods (called by game systems, not UI) ---------------------

    public void MarkCompleted()
    {
        IsCompleted    = true;
        CompletedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void IncrementSkipCount()  => SkipCount++;
    public void IncrementRerollCount() => RerollCount++;

    /// <summary>
    /// Apply recommendation metadata from a provider result.
    /// Called by TaskGenerator after the recommendation layer runs.
    /// </summary>
    public void ApplyRecommendationMetadata(
        float score, string reason, string[] tags,
        float difficultyModifier = 1f, float rewardModifier = 1f)
    {
        RecommendationScore  = score;
        RecommendationReason = reason;
        SourceTags           = tags ?? Array.Empty<string>();
        DifficultyModifier   = difficultyModifier;
        RewardModifier       = rewardModifier;
    }

    // -- Serialization ---------------------------------------------------------

    public TaskInstanceSaveData ToSaveData()
    {
        return new TaskInstanceSaveData
        {
            runtimeId            = RuntimeId,
            definitionId         = DefinitionId,
            generatedAtUtc       = GeneratedAtUtc,
            isCompleted          = IsCompleted,
            completedAtUtc       = CompletedAtUtc,
            skipCount            = SkipCount,
            rerollCount          = RerollCount,
            difficultyModifier   = DifficultyModifier,
            rewardModifier       = RewardModifier,
            recommendationScore  = RecommendationScore,
            recommendationReason = RecommendationReason,
            sourceTags           = SourceTags
        };
    }
}

/// <summary>
/// JSON-serializable snapshot of a <see cref="TaskInstance"/>.
/// Stored inside <see cref="GameSaveData"/>.
/// </summary>
[Serializable]
public class TaskInstanceSaveData
{
    public string   runtimeId;
    public string   definitionId;
    public long     generatedAtUtc;
    public bool     isCompleted;
    public long     completedAtUtc;
    public int      skipCount;
    public int      rerollCount;
    public float    difficultyModifier  = 1f;
    public float    rewardModifier      = 1f;
    public float    recommendationScore = -1f;
    public string   recommendationReason = "";
    public string[] sourceTags;
}
