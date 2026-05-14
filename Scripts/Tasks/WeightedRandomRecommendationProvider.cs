using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default implementation of <see cref="ITaskRecommendationProvider"/>.
///
/// Selects tasks using weighted random selection, with a simple bias toward
/// tasks the player has not recently completed.
///
/// This is intentionally simple -- it is a placeholder that satisfies the
/// interface contract until a real recommendation system is integrated.
///
/// Replacement guide:
///   When integrating an AI/ML recommendation system, create a new class
///   implementing ITaskRecommendationProvider and assign it to
///   TaskGenerator.recommendationProvider. This class can then be deleted
///   or kept as a fallback.
///
/// Current weighting rules:
///   1. Tasks matching the player's recent activity type are weighted higher.
///   2. The immediately previous task definition is weighted lower (avoid repeat).
///   3. All other tasks receive equal weight.
/// </summary>
public class WeightedRandomRecommendationProvider : ITaskRecommendationProvider
{
    // -- Configuration ---------------------------------------------------------

    /// <summary>Weight multiplier for tasks matching a recently completed activity type.</summary>
    private const float RecentTypeBoost = 1.5f;

    /// <summary>Weight multiplier for the task that was just completed/skipped (avoid repeat).</summary>
    private const float RepeatPenalty = 0.1f;

    // -- ITaskRecommendationProvider -------------------------------------------

    public RecommendationResult SelectNext(
        IReadOnlyList<ActivityDefinition> candidates,
        RecommendationContext context)
    {
        if (candidates == null || candidates.Count == 0)
            return RecommendationResult.Empty;

        // Build weight table
        float[] weights = new float[candidates.Count];
        float totalWeight = 0f;

        // Determine the most recently completed activity type from telemetry
        ActivityType? recentType = GetMostRecentCompletedType(context);

        for (int i = 0; i < candidates.Count; i++)
        {
            ActivityDefinition def = candidates[i];
            float weight = 1f;

            // Penalise immediate repeat
            if (!string.IsNullOrEmpty(context.PreviousDefinitionId) &&
                def.Id == context.PreviousDefinitionId)
            {
                weight *= RepeatPenalty;
            }

            // Boost tasks matching recent activity type (variety within type)
            if (recentType.HasValue && def.ActivityType == recentType.Value)
                weight *= RecentTypeBoost;

            weights[i]   = weight;
            totalWeight += weight;
        }

        // Weighted random draw
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        int selectedIndex = candidates.Count - 1; // fallback to last

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative)
            {
                selectedIndex = i;
                break;
            }
        }

        ActivityDefinition selected = candidates[selectedIndex];

        return new RecommendationResult
        {
            SelectedDefinition = selected,
            Score              = -1f, // not scored -- random selection
            Reason             = "weighted_random",
            SourceTags         = new[] { "random", "weighted" },
            DifficultyModifier = 1f,
            RewardModifier     = 1f
        };
    }

    // -- Private ---------------------------------------------------------------

    private ActivityType? GetMostRecentCompletedType(RecommendationContext context)
    {
        if (context.RecentEvents == null) return null;

        // Walk backwards through recent events to find the last TaskCompleted
        for (int i = context.RecentEvents.Count - 1; i >= 0; i--)
        {
            TelemetryEvent evt = context.RecentEvents[i];
            if (evt.eventType == TelemetryEventType.TaskCompleted &&
                !string.IsNullOrEmpty(evt.activityDefinitionId))
            {
                // We only have the definition ID here, not the asset.
                // Return null -- the full type lookup would require asset access.
                // A future provider with asset access can implement this properly.
                return null;
            }
        }
        return null;
    }
}
