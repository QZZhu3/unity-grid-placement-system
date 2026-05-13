using UnityEngine;

/// <summary>
/// Central manager for all player activities.
///
/// Responsibilities:
///   - Receives activity completion signals from UI or external systems
///   - Routes rewards through RewardManager (never grants rewards directly)
///   - Fires events for UI and analytics listeners
///   - Acts as the single entry point for all activity types
///
/// Architecture:
///   UI scripts → ActivityManager.CompleteActivity(definition)
///                      ↓
///              RewardManager.CompleteTask(xpMultiplier) × chestProgressTicks
///                      ↓
///              OnRewardGranted fired → ProgressionRewardListener + ChestRewardListener
///
/// Attach to: ProgressionSystem (alongside RewardManager)
/// </summary>
public class ActivityManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private RewardManager rewardManager;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when any activity is completed. Arg: the completed definition.</summary>
    public event System.Action<ActivityDefinition> OnActivityCompleted;

    /// <summary>Fired specifically when a Focus Session completes.</summary>
    public event System.Action<FocusSessionDefinition> OnFocusSessionCompleted;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (rewardManager == null)
            rewardManager = FindAnyObjectByType<RewardManager>();

        if (rewardManager == null)
            Debug.LogError("[ActivityManager] RewardManager not found. " +
                           "Activities will not grant rewards.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this when any activity is completed.
    /// Routes rewards through RewardManager and fires completion events.
    /// </summary>
    /// <param name="definition">The ActivityDefinition asset for the completed activity.</param>
    public void CompleteActivity(ActivityDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogWarning("[ActivityManager] CompleteActivity called with null definition.");
            return;
        }

        Debug.Log($"[ActivityManager] Activity completed: '{definition.DisplayName}' " +
                  $"(type: {definition.ActivityType})");

        // Grant rewards through RewardManager only — never directly here.
        GrantRewards(definition.RewardConfig);

        // Fire generic completion event
        OnActivityCompleted?.Invoke(definition);

        // Fire type-specific events
        if (definition is FocusSessionDefinition focusDef)
            OnFocusSessionCompleted?.Invoke(focusDef);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void GrantRewards(ActivityRewardConfig config)
    {
        if (rewardManager == null) return;

        // Each chest progress tick is one CompleteTask call, with the XP multiplier applied.
        // RewardManager now supports CompleteTask(float xpMultiplier) natively.
        int ticks = Mathf.Max(1, config.chestProgressTicks);
        float multiplier = Mathf.Max(0f, config.xpMultiplier);

        for (int i = 0; i < ticks; i++)
            rewardManager.CompleteTask(multiplier);
    }
}
