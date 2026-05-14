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
/// Architecture (updated for TaskInstance pipeline):
///
///   TaskRowUI (hold complete) -> ActivityManager.CompleteActivity(TaskInstance)
///                                        |
///                          GrantRewards(definition.RewardConfig, modifiers)
///                                        |
///                          RewardManager.CompleteTask(xpMultiplier * rewardModifier)
///                                        |
///                          OnActivityCompleted / OnTaskInstanceCompleted fired
///                                        |
///                          TaskGenerator.OnTaskCompleted(instance) -> next task
///
/// Backward compatibility:
///   CompleteActivity(ActivityDefinition) still works for FocusSessionRunner
///   and any other callers that do not yet have a TaskInstance.
///
/// Attach to: ProgressionSystem (alongside RewardManager, TaskGenerator)
/// </summary>
public class ActivityManager : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private RewardManager  rewardManager;
    [SerializeField] private TaskGenerator  taskGenerator;

    // -- Events ----------------------------------------------------------------

    /// <summary>
    /// Fired when any activity is completed via the legacy definition path.
    /// Arg: the completed definition.
    /// Kept for backward compatibility with FocusSessionRunner and other systems.
    /// </summary>
    public event System.Action<ActivityDefinition> OnActivityCompleted;

    /// <summary>
    /// Fired when a task is completed via the TaskInstance pipeline.
    /// Arg: the completed TaskInstance (carries definition + runtime metadata).
    /// New systems should subscribe to this instead of OnActivityCompleted.
    /// </summary>
    public event System.Action<TaskInstance> OnTaskInstanceCompleted;

    /// <summary>Fired specifically when a Focus Session completes.</summary>
    public event System.Action<FocusSessionDefinition> OnFocusSessionCompleted;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (rewardManager == null)
            rewardManager = FindAnyObjectByType<RewardManager>();
        if (taskGenerator == null)
            taskGenerator = FindAnyObjectByType<TaskGenerator>();

        if (rewardManager == null)
            Debug.LogError("[ActivityManager] RewardManager not found. " +
                           "Activities will not grant rewards.");
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Complete an activity via the TaskInstance pipeline.
    /// This is the preferred path for all task completions going forward.
    /// Applies reward modifiers from the instance (set by the recommendation layer).
    /// </summary>
    public void CompleteActivity(TaskInstance instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("[ActivityManager] CompleteActivity(TaskInstance) called with null instance.");
            return;
        }

        ActivityDefinition definition = instance.Definition;
        if (definition == null)
        {
            Debug.LogWarning($"[ActivityManager] TaskInstance '{instance.RuntimeId}' has no Definition. " +
                             "Cannot grant rewards.");
            return;
        }

        Debug.Log($"[ActivityManager] Task instance completed: '{definition.DisplayName}' " +
                  $"(runtimeId: {instance.RuntimeId}, " +
                  $"rewardMod: {instance.RewardModifier:F2}, " +
                  $"difficultyMod: {instance.DifficultyModifier:F2})");

        // Apply reward modifiers from the recommendation layer
        GrantRewards(definition.RewardConfig, instance.RewardModifier);

        // Notify TaskGenerator to advance to the next task
        taskGenerator?.OnTaskCompleted(instance);

        // Fire events
        OnTaskInstanceCompleted?.Invoke(instance);
        OnActivityCompleted?.Invoke(definition); // backward compat

        if (definition is FocusSessionDefinition focusDef)
            OnFocusSessionCompleted?.Invoke(focusDef);
    }

    /// <summary>
    /// Complete an activity via the legacy ActivityDefinition path.
    /// Used by FocusSessionRunner and any system that does not yet have a TaskInstance.
    /// Reward modifiers are not applied (modifier = 1.0).
    /// </summary>
    public void CompleteActivity(ActivityDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogWarning("[ActivityManager] CompleteActivity(ActivityDefinition) called with null definition.");
            return;
        }

        Debug.Log($"[ActivityManager] Activity completed (legacy path): '{definition.DisplayName}' " +
                  $"(type: {definition.ActivityType})");

        GrantRewards(definition.RewardConfig, rewardModifier: 1f);

        OnActivityCompleted?.Invoke(definition);

        if (definition is FocusSessionDefinition focusDef)
            OnFocusSessionCompleted?.Invoke(focusDef);
    }

    // -- Private ---------------------------------------------------------------

    private void GrantRewards(ActivityRewardConfig config, float rewardModifier = 1f)
    {
        if (rewardManager == null) return;

        int   ticks      = Mathf.Max(1, config.chestProgressTicks);
        float multiplier = Mathf.Max(0f, config.xpMultiplier * rewardModifier);

        for (int i = 0; i < ticks; i++)
            rewardManager.CompleteTask(multiplier);
    }
}
