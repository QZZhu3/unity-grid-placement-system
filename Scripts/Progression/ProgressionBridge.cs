using UnityEngine;

/// <summary>
/// Connects the placement system to the progression and chest reward systems.
///
/// Attach this to the ProgressionSystem GameObject. It auto-discovers
/// PlacementManager, PlayerProgressionManager, and RewardManager at startup.
///
/// What it does:
///   - Routes every placement through RewardManager.CompleteTask(), which
///     awards XP AND advances chest progress in one call.
///   - Logs XP and level changes to the Console.
///
/// To test quickly:
///   - Right-click the RewardManager component -> "Debug: Complete Task"
///     to simulate a task without placing items.
///   - Right-click the PlayerProgressionManager component -> "Debug: Add 100 XP"
///     to fast-forward through levels.
/// </summary>
public class ProgressionBridge : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private PlacementManager          placementManager;
    [SerializeField] private PlayerProgressionManager  progressionManager;
    [SerializeField] private RewardManager             rewardManager;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (placementManager   == null) placementManager   = FindAnyObjectByType<PlacementManager>();
        if (progressionManager == null) progressionManager = FindAnyObjectByType<PlayerProgressionManager>();
        if (rewardManager      == null) rewardManager      = FindAnyObjectByType<RewardManager>();

        if (placementManager == null)
            Debug.LogWarning("[ProgressionBridge] PlacementManager not found in scene.");
        if (progressionManager == null)
            Debug.LogWarning("[ProgressionBridge] PlayerProgressionManager not found in scene.");
        if (rewardManager == null)
            Debug.LogWarning("[ProgressionBridge] RewardManager not found -- chest progress will not advance.");
    }

    private void OnEnable()
    {
        if (placementManager != null)
            placementManager.OnItemPlaced += HandleItemPlaced;

        if (progressionManager != null)
        {
            progressionManager.OnLevelUp  += HandleLevelUp;
            progressionManager.OnXpGained += HandleXpGained;
        }
    }

    private void OnDisable()
    {
        if (placementManager != null)
            placementManager.OnItemPlaced -= HandleItemPlaced;

        if (progressionManager != null)
        {
            progressionManager.OnLevelUp  -= HandleLevelUp;
            progressionManager.OnXpGained -= HandleXpGained;
        }
    }

    // -- Event handlers --------------------------------------------------------

    private void HandleItemPlaced(PlacedItem item)
    {
        // Route through RewardManager so both XP and chest progress are awarded.
        if (rewardManager != null)
        {
            rewardManager.CompleteTask();
        }
        else if (progressionManager != null)
        {
            // Fallback: award XP directly if RewardManager is missing.
            progressionManager.AddXp(10f);
        }
    }

    private void HandleXpGained(float newXp)
    {
        Debug.Log($"[Progression] XP: {newXp:F0}  |  Level: {progressionManager.CurrentLevel}");
    }

    private void HandleLevelUp(int newLevel)
    {
        Debug.Log($"[Progression] *** LEVEL UP! Now level {newLevel} ***");
    }
}
