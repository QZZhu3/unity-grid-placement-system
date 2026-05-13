using System.Collections.Generic;
using UnityEngine;
using PlacementSystem.SaveSystem;

/// <summary>
/// Handles save and load for the chest system.
///
/// Integrates with the existing <see cref="ISaveable"/> pattern used by
/// <see cref="ProgressionSaveHandler"/> and <see cref="InventorySaveHandler"/>.
///
/// Saves:
///   - Current task progress toward the next chest
///   - Ordered list of pending chest IDs in the queue
///
/// Loads:
///   - Restores task progress without triggering chest-earned events
///   - Restores pending chest queue using chest ID -> asset lookup
///
/// Attach to: ProgressionSystem (or ChestSystem GameObject alongside the other chest components).
/// </summary>
public class ChestSaveHandler : MonoBehaviour, ISaveable
{
    // -- Inspector -------------------------------------------------------------
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ChestProgressManager chestProgress;
    [SerializeField] private ChestQueueManager    chestQueue;

    [Header("Chest Registry")]
    [Tooltip("All ChestDefinition assets in the game. Used to resolve IDs on load.")]
    [SerializeField] private List<ChestDefinition> allChests = new List<ChestDefinition>();

    // -- Lifecycle -------------------------------------------------------------
    private void Awake()
    {
        if (chestProgress == null)
            chestProgress = FindAnyObjectByType<ChestProgressManager>();
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();
    }

    // -- ISaveable -------------------------------------------------------------

    public void PopulateSaveData(GameSaveData data)
    {
        data.chestData.currentProgress  = chestProgress != null ? chestProgress.CurrentProgress : 0;
        data.chestData.pendingChestIds   = chestQueue    != null ? chestQueue.GetQueueIds()      : new List<string>();
    }

    public void LoadFromSaveData(GameSaveData data)
    {
        if (data.chestData == null) return;

        // Restore task progress (no events fired)
        if (chestProgress != null)
            chestProgress.LoadProgress(data.chestData.currentProgress);

        // Restore pending chest queue
        if (chestQueue != null && data.chestData.pendingChestIds != null)
            chestQueue.LoadQueue(data.chestData.pendingChestIds, ResolveChestId);
    }

    // -- Private helpers -------------------------------------------------------

    private ChestDefinition ResolveChestId(string id)
    {
        foreach (ChestDefinition chest in allChests)
        {
            if (chest != null && chest.Id == id)
                return chest;
        }
        Debug.LogWarning($"[ChestSaveHandler] Could not find ChestDefinition with ID '{id}'.");
        return null;
    }
}
