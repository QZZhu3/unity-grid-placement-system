using UnityEngine;

/// <summary>
/// Centralized reward distributor.
///
/// Acts as the coordinator between the task/progression layer and the chest/reward layer.
/// All reward-granting flows pass through this class so that UI, analytics, and
/// future systems only need to subscribe to one place.
///
/// Responsibilities:
///   - Accept task completion signals and distribute XP + chest progress
///   - Open chests by drawing rewards and adding them to inventory
///   - Fire events for UI and analytics to consume
///
/// PlayerProgressionManager and ChestProgressManager must NOT know about each other.
/// RewardManager is the bridge.
///
/// Attach to: ProgressionSystem (or a dedicated ChestSystem GameObject).
/// </summary>
public class RewardManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private PlayerProgressionManager progressionManager;
    [SerializeField] private ChestProgressManager     chestProgress;
    [SerializeField] private ChestQueueManager        chestQueue;
    [SerializeField] private ItemRewardPool           rewardPool;
    [SerializeField] private InventoryManager         inventoryManager;

    [Header("Task Reward Settings")]
    [Tooltip("XP granted per completed task.")]
    [SerializeField, Min(0f)] private float xpPerTask = 50f;

    [Tooltip("Chest progress granted per completed task (usually 1).")]
    [SerializeField, Min(1)] private int chestProgressPerTask = 1;

    [Header("Active Season (optional)")]
    [Tooltip("Current active season. Leave empty for no seasonal filtering.")]
    [SerializeField] private SeasonTag activeSeason;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private RewardDrawService drawService;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when a task is completed. Arg: the XP amount granted.</summary>
    public event System.Action<float> OnTaskCompleted;

    /// <summary>Fired when a chest is successfully opened with rewards.</summary>
    public event System.Action<ChestOpenResult> OnChestOpened;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (progressionManager == null)
            progressionManager = FindAnyObjectByType<PlayerProgressionManager>();
        if (chestProgress == null)
            chestProgress = FindAnyObjectByType<ChestProgressManager>();
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
    /// Grants XP and chest progress. PlayerProgressionManager is not called directly
    /// from task systems — all task rewards flow through here.
    /// </summary>
    public void CompleteTask()
    {
        Debug.Log("[RewardManager] Task completed.");

        // Grant XP
        if (progressionManager != null && xpPerTask > 0f)
            progressionManager.AddXp(xpPerTask);

        // Grant chest progress
        if (chestProgress != null)
            chestProgress.AddProgress(chestProgressPerTask);

        OnTaskCompleted?.Invoke(xpPerTask);
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
            chest:         chest,
            activeSeason:  activeSeason);

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
