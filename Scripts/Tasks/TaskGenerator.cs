using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates <see cref="TaskInstance"/> objects from a pool of
/// <see cref="ActivityDefinition"/> assets via the recommendation layer.
///
/// Architecture role (the full pipeline):
///
///   ActivityDefinition assets (ScriptableObjects, static data)
///         |
///         v
///   TaskGenerator.GenerateNext()
///         |
///         v
///   ITaskRecommendationProvider.SelectNext()   <-- swap this for AI later
///         |
///         v
///   TaskInstance created with recommendation metadata applied
///         |
///         v
///   OnTaskGenerated event fired
///         |
///         v
///   TaskJournalPanel / UI displays the instance
///
/// Design constraints:
///   - UI never calls this directly to decide which task to show.
///   - ActivityManager never calls this -- it only receives completed instances.
///   - Only this class creates TaskInstances.
///
/// Attach to: ProgressionSystem (alongside ActivityManager)
/// </summary>
public class TaskGenerator : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Task Pool")]
    [Tooltip("All ActivityDefinition assets available for recommendation. " +
             "Drag all your task ScriptableObjects here.")]
    [SerializeField] private List<ActivityDefinition> taskPool = new List<ActivityDefinition>();

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private PlayerProgressionManager progressionManager;
    [SerializeField] private PlayerTelemetryManager telemetryManager;

    // -- Events ----------------------------------------------------------------

    /// <summary>
    /// Fired when a new TaskInstance has been generated and is ready for display.
    /// The Journal UI subscribes to this to update its display.
    /// </summary>
    public event System.Action<TaskInstance> OnTaskGenerated;

    // -- Runtime state ---------------------------------------------------------

    private ITaskRecommendationProvider recommendationProvider;
    private TaskInstance currentInstance;
    private string previousDefinitionId = "";
    private int sessionTasksCompleted = 0;
    private int sessionTasksSkipped   = 0;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (progressionManager == null)
            progressionManager = FindAnyObjectByType<PlayerProgressionManager>();
        if (telemetryManager == null)
            telemetryManager = FindAnyObjectByType<PlayerTelemetryManager>();

        // Default to weighted random -- swap this assignment to use a different provider
        recommendationProvider = new WeightedRandomRecommendationProvider();
    }

    private void Start()
    {
        // Only generate if no instance was injected by TaskSaveHandler on load
        if (currentInstance == null)
            GenerateNext();
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Generate the next task and fire <see cref="OnTaskGenerated"/>.
    /// Called by: session start, task completion, task skip, task reroll.
    /// </summary>
    public void GenerateNext()
    {
        if (taskPool == null || taskPool.Count == 0)
        {
            Debug.LogWarning("[TaskGenerator] Task pool is empty. Cannot generate a task.");
            return;
        }

        RecommendationContext context = BuildContext();
        RecommendationResult result = recommendationProvider.SelectNext(taskPool, context);

        if (result.IsEmpty)
        {
            Debug.LogWarning("[TaskGenerator] Recommendation provider returned empty result. " +
                             "Falling back to random selection.");
            result = FallbackRandom(context);
        }

        if (result.IsEmpty)
        {
            Debug.LogError("[TaskGenerator] Fallback random also failed. Task pool may be empty.");
            return;
        }

        // Create the TaskInstance
        TaskInstance instance = new TaskInstance(result.SelectedDefinition);
        instance.ApplyRecommendationMetadata(
            result.Score,
            result.Reason,
            result.SourceTags,
            result.DifficultyModifier,
            result.RewardModifier);

        currentInstance = instance;
        previousDefinitionId = instance.DefinitionId;

        // Record telemetry
        telemetryManager?.Record(new TelemetryEvent
        {
            eventType            = TelemetryEventType.TaskGenerated,
            timestampUtc         = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            taskRuntimeId        = instance.RuntimeId,
            activityDefinitionId = instance.DefinitionId,
            tag                  = result.Reason
        });

        Debug.Log($"[TaskGenerator] Generated task: '{result.SelectedDefinition.DisplayName}' " +
                  $"(reason: {result.Reason}, score: {result.Score:F2})");

        OnTaskGenerated?.Invoke(instance);
    }

    /// <summary>
    /// Called when the player skips the current task without completing it.
    /// Records telemetry and generates a new task.
    /// </summary>
    public void SkipCurrentTask()
    {
        if (currentInstance != null)
        {
            currentInstance.IncrementSkipCount();
            sessionTasksSkipped++;

            int level = progressionManager?.CurrentLevel ?? 0;
            telemetryManager?.Record(TelemetryEvent.TaskSkipped(currentInstance, level));

            Debug.Log($"[TaskGenerator] Task skipped: '{currentInstance.DefinitionId}' " +
                      $"(skip #{currentInstance.SkipCount})");
        }
        GenerateNext();
    }

    /// <summary>
    /// Called when the player rerolls (replaces) the current task.
    /// Records telemetry and generates a new task.
    /// </summary>
    public void RerollCurrentTask()
    {
        if (currentInstance != null)
        {
            currentInstance.IncrementRerollCount();
            sessionTasksSkipped++;

            int level = progressionManager?.CurrentLevel ?? 0;
            telemetryManager?.Record(TelemetryEvent.TaskRerolled(currentInstance, level));

            Debug.Log($"[TaskGenerator] Task rerolled: '{currentInstance.DefinitionId}' " +
                      $"(reroll #{currentInstance.RerollCount})");
        }
        GenerateNext();
    }

    /// <summary>
    /// Called by ActivityManager after a task is completed.
    /// Records telemetry and generates the next task.
    /// </summary>
    public void OnTaskCompleted(TaskInstance instance)
    {
        if (instance == null) return;

        instance.MarkCompleted();
        sessionTasksCompleted++;

        int level = progressionManager?.CurrentLevel ?? 0;
        telemetryManager?.Record(TelemetryEvent.TaskCompleted(instance, level));

        GenerateNext();
    }

    /// <summary>
    /// Returns the currently active TaskInstance, or null if none has been generated.
    /// </summary>
    public TaskInstance GetCurrentInstance() => currentInstance;

    /// <summary>
    /// Inject a TaskInstance restored from save data.
    /// Called by TaskSaveHandler before Start() fires.
    /// Prevents generating a new task when one already exists from a previous session.
    /// </summary>
    public void InjectRestoredInstance(TaskInstance instance)
    {
        if (instance == null) return;
        currentInstance      = instance;
        previousDefinitionId = instance.DefinitionId;
        Debug.Log($"[TaskGenerator] Restored instance injected: '{instance.DefinitionId}'");
        OnTaskGenerated?.Invoke(instance);
    }

    /// <summary>
    /// Replace the recommendation provider at runtime.
    /// Use this to swap in an AI provider after it has been initialized.
    /// </summary>
    public void SetRecommendationProvider(ITaskRecommendationProvider provider)
    {
        if (provider == null)
        {
            Debug.LogWarning("[TaskGenerator] Attempted to set null recommendation provider. Ignored.");
            return;
        }
        recommendationProvider = provider;
        Debug.Log($"[TaskGenerator] Recommendation provider changed to: {provider.GetType().Name}");
    }

    // -- Private ---------------------------------------------------------------

    private RecommendationContext BuildContext()
    {
        return new RecommendationContext
        {
            PlayerLevel             = progressionManager?.CurrentLevel ?? 0,
            PlayerXp                = progressionManager?.CurrentXp ?? 0f,
            RecentEvents            = telemetryManager?.GetRecentHistory(),
            PreviousDefinitionId    = previousDefinitionId,
            PreviousTaskRuntimeId   = currentInstance?.RuntimeId ?? "",
            SessionTasksCompleted   = sessionTasksCompleted,
            SessionTasksSkipped     = sessionTasksSkipped
        };
    }

    private RecommendationResult FallbackRandom(RecommendationContext context)
    {
        if (taskPool == null || taskPool.Count == 0)
            return RecommendationResult.Empty;

        ActivityDefinition def = taskPool[Random.Range(0, taskPool.Count)];
        return new RecommendationResult
        {
            SelectedDefinition = def,
            Score              = -1f,
            Reason             = "fallback_random",
            SourceTags         = new[] { "fallback" },
            DifficultyModifier = 1f,
            RewardModifier     = 1f
        };
    }
}
