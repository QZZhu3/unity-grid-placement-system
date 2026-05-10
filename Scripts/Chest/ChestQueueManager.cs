using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the queue of earned but unopened chests.
///
/// Chests are added by <see cref="ChestProgressManager"/> when earned,
/// and consumed by <see cref="RewardManager"/> when opened.
///
/// This class is responsible only for queue management.
/// It has no knowledge of reward drawing or UI.
///
/// Attach to: ProgressionSystem (or a dedicated ChestSystem GameObject).
/// </summary>
public class ChestQueueManager : MonoBehaviour
{
    // ── Runtime state ─────────────────────────────────────────────────────────
    private readonly Queue<ChestDefinition> pendingChests = new Queue<ChestDefinition>();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when a new chest is added to the queue.</summary>
    public event System.Action<ChestDefinition> OnChestEnqueued;

    /// <summary>Fired when a chest is dequeued for opening.</summary>
    public event System.Action<ChestDefinition> OnChestDequeued;

    /// <summary>Fired whenever the queue count changes.</summary>
    public event System.Action<int> OnQueueCountChanged;

    // ── Public accessors ──────────────────────────────────────────────────────

    /// <summary>Number of unopened chests waiting in the queue.</summary>
    public int PendingCount => pendingChests.Count;

    /// <summary>True if there is at least one chest ready to open.</summary>
    public bool HasPendingChests => pendingChests.Count > 0;

    /// <summary>Peek at the next chest without removing it. Returns null if empty.</summary>
    public ChestDefinition PeekNext() =>
        pendingChests.Count > 0 ? pendingChests.Peek() : null;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a chest to the end of the queue.
    /// Called by <see cref="ChestProgressManager"/> when a chest is earned.
    /// </summary>
    public void EnqueueChest(ChestDefinition chest)
    {
        if (chest == null) return;
        pendingChests.Enqueue(chest);
        Debug.Log($"[ChestQueueManager] Enqueued '{chest.DisplayName}'. Queue size: {pendingChests.Count}");
        OnChestEnqueued?.Invoke(chest);
        OnQueueCountChanged?.Invoke(pendingChests.Count);
    }

    /// <summary>
    /// Removes and returns the next chest from the queue.
    /// Returns null if the queue is empty.
    /// Called by <see cref="RewardManager"/> when the player opens a chest.
    /// </summary>
    public ChestDefinition DequeueChest()
    {
        if (pendingChests.Count == 0) return null;
        ChestDefinition chest = pendingChests.Dequeue();
        Debug.Log($"[ChestQueueManager] Dequeued '{chest.DisplayName}'. Queue size: {pendingChests.Count}");
        OnChestDequeued?.Invoke(chest);
        OnQueueCountChanged?.Invoke(pendingChests.Count);
        return chest;
    }

    /// <summary>
    /// Loads the queue from a list of chest IDs and a lookup dictionary.
    /// Called by the save system on load.
    /// </summary>
    public void LoadQueue(IEnumerable<string> chestIds, System.Func<string, ChestDefinition> resolver)
    {
        pendingChests.Clear();
        foreach (string id in chestIds)
        {
            ChestDefinition chest = resolver(id);
            if (chest != null)
                pendingChests.Enqueue(chest);
            else
                Debug.LogWarning($"[ChestQueueManager] Could not resolve chest ID '{id}' on load.");
        }
        OnQueueCountChanged?.Invoke(pendingChests.Count);
    }

    /// <summary>
    /// Returns all pending chest IDs for serialization.
    /// </summary>
    public List<string> GetQueueIds()
    {
        List<string> ids = new List<string>();
        foreach (ChestDefinition chest in pendingChests)
            ids.Add(chest.Id);
        return ids;
    }

    // ── Debug ─────────────────────────────────────────────────────────────────
    [ContextMenu("Debug: Print Queue")]
    private void DebugPrint()
    {
        Debug.Log($"[ChestQueueManager] Pending chests ({pendingChests.Count}):");
        foreach (ChestDefinition chest in pendingChests)
            Debug.Log($"  - {chest.DisplayName} ({chest.Id})");
    }
}
