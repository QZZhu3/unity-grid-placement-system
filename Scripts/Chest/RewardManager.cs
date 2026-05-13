using UnityEngine;

/// <summary>
/// Centralized reward coordinator.
///
/// RewardManager is an event emitter only. It does not call AddXp or AddProgress directly.
/// Instead it fires <see cref="OnRewardGranted"/> and lets downstream systems react
/// independently. This keeps RewardManager decoupled from progression and chest logic.
///
/// Reward flow:
///   Caller → CompleteTask(xpMultiplier)
///                ↓
///          OnRewardGranted(xp, chestTicks) fired
///                ↓
///   ProgressionRewardListener → PlayerProgressionManager.AddXp()
///   ChestRewardListener       → ChestProgressManager.AddProgress()
///
/// Chest opening flow:
///   Caller → OpenNextChest()
///                ↓
///          OnChestOpened(ChestOpenResult) fired
///                ↓
///   UI / analytics subscribe and react
///
/// Attach to: ProgressionSystem (or a dedicated ChestSystem GameObject).
/// </summary>
public class RewardManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ChestQueueManager chestQueue;
    [SerializeField] private ItemRewardPool    rewardPool;
    [SerializeField] private InventoryManager  inventoryManager;

    [Header("Task Reward Settings")]
    [Tooltip("Base XP granted per completed task.")]
    [SerializeField, Min(0f)] private float xpPerTask = 50f;

    [Tooltip("Chest progress ticks granted per completed task (usually 1).")]
    [SerializeField, Min(1)] private int chestProgressPerTask = 1;

    [Header("Active Season (optional)")]
    [Tooltip("Current active season. Leave empty for no seasonal filtering.")]
    [SerializeField] private SeasonTag activeSeason;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private RewardDrawService drawService;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired when a task is completed.
    /// Args: (xpAmount, chestProgressTicks).
    /// Downstream systems (ProgressionRewardListener, ChestRewardListener) subscribe here.
    /// </summary>
    public event System.Action<float, int> OnRewardGranted;

    /// <summary>
    /// Fired when a task is completed (legacy / analytics convenience).
    /// Arg: xpAmount granted.
    /// </summary>
    public event System.Action<float> OnTaskCompleted;

    /// <summary>Fired when a chest is successfully opened with rewards.</summary>
    public event System.Action<ChestOpenResult> OnChestOpened;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();
        if (rewardPool == null)
            rewardPool = FindAnyObjectByType<ItemRewardPool>();
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (rewardPool != null)
            drawService = new RewardDrawService(rewardPool);
        else
            Debug.LogWarning("[RewardManager] ItemRewardPool not found. Chest opening will not work.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this when a task is completed.
    /// Fires <see cref="OnRewardGranted"/> and <see cref="OnTaskCompleted"/>.
    /// Does NOT call AddXp or AddProgress directly — listeners handle that.
    /// </summary>
    /// <param name="xpMultiplier">Multiplier applied to the base XP value (default 1).</param>
    public void CompleteTask(float xpMultiplier = 1f)
    {
        float xp = xpPerTask * Mathf.Max(0f, xpMultiplier);
        Debug.Log($"[RewardManager] Task completed. XP: {xp:F0} (x{xpMultiplier}), " +
                  $"Chest ticks: {chestProgressPerTask}");

        OnRewardGranted?.Invoke(xp, chestProgressPerTask);
        OnTaskCompleted?.Invoke(xp);
    }

    /// <summary>
    /// Opens the next chest in the queue, draws rewards, and adds them to inventory.
    /// Fires <see cref="OnChestOpened"/> with the full result for UI to consume.
    /// Returns null if no chests are available or the draw fails.
    /// </summary>
    public ChestOpenResult OpenNextChest()
    {
        if (chestQueue == null || !chestQueue.HasPendingChests)
        {
            Debug.LogWarning("[RewardManager] No chests available to open.");
            return null;
        }

        if (drawService == null)
        {
            Debug.LogError("[RewardManager] RewardDrawService not initialized. Cannot open chest.");
            return null;
        }

        ChestQueueEntry entry = chestQueue.DequeueChest();
        if (entry == null || entry.ChestDefinition == null) return null;
        ChestDefinition chest = entry.ChestDefinition;

        // Build draw context
        var context = new RewardSelectionContext(
            chest:        chest,
            activeSeason: activeSeason);

        // Draw rewards
        RewardBundle bundle = drawService.Draw(context);

        // Add rewards to inventory
        if (inventoryManager != null)
        {
            foreach (RewardResult reward in bundle.Items)
            {
                if (reward.Item != null)
                    inventoryManager.AddItem(reward.Item, reward.Quantity);
            }
        }

        // Build result and fire event
        ChestOpenResult result = new ChestOpenResult(chest, bundle);
        OnChestOpened?.Invoke(result);

        Debug.Log($"[RewardManager] Opened '{chest.DisplayName}' (source: {entry.SourceTag}): " +
                  $"{bundle.TotalItemCount} item(s) rewarded.");

        return result;
    }

    // ── Debug ─────────────────────────────────────────────────────────────────
    [ContextMenu("Debug: Complete Task")]
    private void DebugCompleteTask() => CompleteTask();

    [ContextMenu("Debug: Open Next Chest")]
    private void DebugOpenChest() => OpenNextChest();
}
