using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the queue of earned but unopened chests.
///
/// Uses <see cref="ChestQueueEntry"/> instead of raw <see cref="ChestDefinition"/>
/// to support multiple chest types, seasonal events, and future metadata.
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
    // -- Runtime state ---------------------------------------------------------

    private readonly Queue<ChestQueueEntry> pendingChests = new Queue<ChestQueueEntry>();

    // -- Events ----------------------------------------------------------------

    /// <summary>Fired when a new chest entry is added to the queue.</summary>
    public event System.Action<ChestQueueEntry> OnChestEnqueued;

    /// <summary>Fired when a chest entry is dequeued for opening.</summary>
    public event System.Action<ChestQueueEntry> OnChestDequeued;

    /// <summary>Fired whenever the queue count changes.</summary>
    public event System.Action<int> OnQueueCountChanged;

    // -- Public accessors ------------------------------------------------------

    /// <summary>Number of unopened chests waiting in the queue.</summary>
    public int PendingCount => pendingChests.Count;

    /// <summary>True if there is at least one chest ready to open.</summary>
    public bool HasPendingChests => pendingChests.Count > 0;

    /// <summary>Peek at the next entry without removing it. Returns null if empty.</summary>
    public ChestQueueEntry PeekNext() =>
        pendingChests.Count > 0 ? pendingChests.Peek() : null;

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Adds a chest to the end of the queue using a pre-built entry.
    /// </summary>
    public void EnqueueChest(ChestQueueEntry entry)
    {
        if (entry == null || entry.ChestDefinition == null) return;

        pendingChests.Enqueue(entry);
        Debug.Log($"[ChestQueueManager] Enqueued '{entry.ChestDefinition.DisplayName}' " +
                  $"(source: {entry.SourceTag}). Queue size: {pendingChests.Count}");
        OnChestEnqueued?.Invoke(entry);
        OnQueueCountChanged?.Invoke(pendingChests.Count);
    }

    /// <summary>
    /// Convenience overload: wraps a ChestDefinition in a new entry and enqueues it.
    /// </summary>
    public void EnqueueChest(ChestDefinition chest, string sourceTag = "")
    {
        if (chest == null) return;
        EnqueueChest(new ChestQueueEntry(chest, sourceTag));
    }

    /// <summary>
    /// Removes and returns the next chest entry from the queue.
    /// Returns null if the queue is empty.
    /// Called by <see cref="RewardManager"/> when the player opens a chest.
    /// </summary>
    public ChestQueueEntry DequeueChest()
    {
        if (pendingChests.Count == 0) return null;

        ChestQueueEntry entry = pendingChests.Dequeue();
        Debug.Log($"[ChestQueueManager] Dequeued '{entry.ChestDefinition.DisplayName}'. " +
                  $"Queue size: {pendingChests.Count}");
        OnChestDequeued?.Invoke(entry);
        OnQueueCountChanged?.Invoke(pendingChests.Count);
        return entry;
    }

    /// <summary>
    /// Loads the queue from a list of chest IDs and a resolver function.
    /// Called by the save system on load.
    /// </summary>
    public void LoadQueue(IEnumerable<string> chestIds, System.Func<string, ChestDefinition> resolver)
    {
        pendingChests.Clear();

        foreach (string id in chestIds)
        {
            ChestDefinition chest = resolver(id);
            if (chest != null)
                pendingChests.Enqueue(new ChestQueueEntry(chest, "save_restore"));
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
        foreach (ChestQueueEntry entry in pendingChests)
            ids.Add(entry.ChestDefinition.Id);
        return ids;
    }

    // -- Debug -----------------------------------------------------------------

    [ContextMenu("Debug: Print Queue")]
    private void DebugPrint()
    {
        Debug.Log($"[ChestQueueManager] Pending chests ({pendingChests.Count}):");
        foreach (ChestQueueEntry entry in pendingChests)
            Debug.Log($"  - {entry.ChestDefinition.DisplayName} ({entry.ChestDefinition.Id}) " +
                      $"| source: {entry.SourceTag} | earned: {entry.EarnedAtUtc}");
    }
}
