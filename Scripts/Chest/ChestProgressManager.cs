using UnityEngine;

/// <summary>
/// Tracks progress toward earning the next chest.
///
/// Every time a task is completed (via <see cref="AddProgress"/>), progress increments.
/// When the threshold is reached, a chest is earned and forwarded to
/// <see cref="ChestQueueManager"/>.
///
/// This class is responsible only for progress tracking and chest earning.
/// It has no knowledge of chest opening, rewards, or UI.
///
/// Attach to: ProgressionSystem (or a dedicated ChestSystem GameObject).
/// </summary>
public class ChestProgressManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ChestQueueManager chestQueue;

    [Header("Default Chest")]
    [Tooltip("The chest definition earned when the progress threshold is reached.")]
    [SerializeField] private ChestDefinition defaultChest;

    [Header("Progress Settings")]
    [Tooltip("Number of tasks required to earn one chest.")]
    [SerializeField, Min(1)] private int tasksPerChest = 3;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int currentProgress;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired whenever progress changes. Args: (currentProgress, tasksPerChest).</summary>
    public event System.Action<int, int> OnProgressChanged;

    /// <summary>Fired when a chest is earned. Arg: the chest definition earned.</summary>
    public event System.Action<ChestDefinition> OnChestEarned;

    // ── Public accessors ──────────────────────────────────────────────────────
    public int CurrentProgress => currentProgress;
    public int TasksPerChest   => tasksPerChest;

    /// <summary>Progress as a 0–1 fraction.</summary>
    public float ProgressFraction =>
        tasksPerChest > 0 ? (float)currentProgress / tasksPerChest : 0f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();

        if (chestQueue == null)
            Debug.LogWarning("[ChestProgressManager] ChestQueueManager not found in scene.");

        if (defaultChest == null)
            Debug.LogWarning("[ChestProgressManager] No default chest assigned. " +
                             "Chests will not be earned until one is assigned.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds progress toward the next chest. Call this when a task is completed.
    /// Each call represents one completed task.
    /// </summary>
    /// <param name="amount">Number of tasks completed (default 1).</param>
    public void AddProgress(int amount = 1)
    {
        if (amount <= 0) return;

        currentProgress += amount;
        OnProgressChanged?.Invoke(currentProgress, tasksPerChest);

        // Check for chest(s) earned — handles multi-task completions
        while (currentProgress >= tasksPerChest)
        {
            currentProgress -= tasksPerChest;
            EarnChest(defaultChest);
        }

        // Fire one final progress event after overflow is resolved
        OnProgressChanged?.Invoke(currentProgress, tasksPerChest);
    }

    /// <summary>
    /// Directly sets the current progress value. Used by the save system on load.
    /// Does not fire chest-earned events.
    /// </summary>
    public void LoadProgress(int progress)
    {
        currentProgress = Mathf.Clamp(progress, 0, tasksPerChest - 1);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void EarnChest(ChestDefinition chest)
    {
        if (chest == null)
        {
            Debug.LogWarning("[ChestProgressManager] Cannot earn chest: no chest definition assigned.");
            return;
        }

        Debug.Log($"[ChestProgressManager] Chest earned: '{chest.DisplayName}'");
        chestQueue?.EnqueueChest(chest);
        OnChestEarned?.Invoke(chest);
    }

    // ── Debug ─────────────────────────────────────────────────────────────────
    [ContextMenu("Debug: Add 1 Task Progress")]
    private void DebugAddProgress() => AddProgress(1);

    [ContextMenu("Debug: Complete Full Chest Progress")]
    private void DebugCompleteChest() => AddProgress(tasksPerChest - currentProgress);

    [ContextMenu("Debug: Print Progress")]
    private void DebugPrint() =>
        Debug.Log($"[ChestProgressManager] Progress: {currentProgress}/{tasksPerChest}");
}
