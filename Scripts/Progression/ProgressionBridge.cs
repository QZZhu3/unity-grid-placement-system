using UnityEngine;

/// <summary>
/// Connects the placement system to the progression system.
///
/// Attach this to the ProgressionSystem GameObject. It auto-discovers
/// PlacementManager and PlayerProgressionManager at startup — no manual
/// Inspector wiring is required.
///
/// What it does:
///   - Awards <see cref="xpPerPlacement"/> XP every time an item is placed.
///   - Logs the XP gain and current level to the Console so you can see
///     progression happening in real time.
///
/// To test quickly:
///   - Right-click the PlayerProgressionManager component → "Debug: Add 100 XP"
///     to fast-forward through levels without placing items.
/// </summary>
public class ProgressionBridge : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private PlacementManager          placementManager;
    [SerializeField] private PlayerProgressionManager  progressionManager;

    [Header("XP Settings")]
    [Tooltip("XP awarded to the player each time an item is successfully placed.")]
    [SerializeField] private int xpPerPlacement = 10;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (placementManager   == null) placementManager   = FindAnyObjectByType<PlacementManager>();
        if (progressionManager == null) progressionManager = FindAnyObjectByType<PlayerProgressionManager>();

        if (placementManager == null)
            Debug.LogWarning("[ProgressionBridge] PlacementManager not found in scene.");
        if (progressionManager == null)
            Debug.LogWarning("[ProgressionBridge] PlayerProgressionManager not found in scene.");
    }

    private void OnEnable()
    {
        if (placementManager != null)
            placementManager.OnItemPlaced += HandleItemPlaced;

        if (progressionManager != null)
        {
            progressionManager.OnLevelUp        += HandleLevelUp;
            progressionManager.OnXpGained       += HandleXpGained;
        }
    }

    private void OnDisable()
    {
        if (placementManager != null)
            placementManager.OnItemPlaced -= HandleItemPlaced;

        if (progressionManager != null)
        {
            progressionManager.OnLevelUp        -= HandleLevelUp;
            progressionManager.OnXpGained       -= HandleXpGained;
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleItemPlaced(PlacedItem item)
    {
        if (progressionManager == null) return;
        progressionManager.AddXp(xpPerPlacement);
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
