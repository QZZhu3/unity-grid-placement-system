using UnityEngine;
using System.Collections;
using PlacementSystem.SaveSystem;

/// <summary>
/// Test driver for the inventory-integrated placement system.
///
/// Behaviour:
///   - If NO save file exists: auto-starts placement on Play.
///   - If a save file exists: skips auto-start. The save system restores state,
///     so we don't want to spawn an extra draggable on top.
///   - After each successful placement, immediately starts the next one (if qty > 0).
///   - After a pick-up cancel, starts a fresh placement if qty > 0.
///   - After a basket return: does NOT auto-start (user selects from Inventory UI).
/// </summary>
public class DragTest : MonoBehaviour
{
    public PlaceableItem testItem;

    private PlacementManager manager;
    private InventoryManager inventory;
    private SaveManager      saveManager;

    void Start()
    {
        manager     = GetComponent<PlacementManager>();
        inventory   = FindAnyObjectByType<InventoryManager>();
        saveManager = FindAnyObjectByType<SaveManager>();

        // Subscribe to events
        manager.OnItemPlaced         += OnItemPlaced;
        manager.OnItemPickedUp       += OnItemPickedUp;
        manager.OnPlacementCancelled += OnPlacementCancelled;

        // Only auto-start if there is no existing save file.
        // When a save file exists, the player presses Force Load (or the game loads
        // automatically) and the save system restores the correct state — we must
        // not spawn an extra draggable on top of that.
        bool hasSave = saveManager != null && saveManager.HasSaveFile;
        if (!hasSave)
        {
            TryBeginPlacement();
        }
        else
        {
            Debug.Log("[DragTest] Save file detected — skipping auto-start. Press Force Load to restore.");
        }
    }

    private void OnItemPlaced(PlacedItem item)
    {
        // After placing, immediately start the next placement if we still have stock
        StartCoroutine(BeginNextFrame());
    }

    private void OnItemPickedUp(PlacedItem item)
    {
        // Item is now being dragged again — PlacementManager handles this automatically.
    }

    private void OnPlacementCancelled(PlacedItem item)
    {
        // Pick-up was cancelled — item went back to the grid.
        // Start a fresh placement if we still have stock.
        StartCoroutine(BeginNextFrame());
    }

    private IEnumerator BeginNextFrame()
    {
        // Wait two frames: one for PlacementManager cleanup, one for the cooldown to clear
        yield return null;
        yield return null;
        TryBeginPlacement();
    }

    private void TryBeginPlacement()
    {
        if (inventory != null && !inventory.HasItem(testItem.ItemId))
        {
            Debug.Log("[DragTest] Out of stock — no more placements.");
            return;
        }

        if (!manager.IsDragging)
            manager.BeginPlacement(testItem);
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnItemPlaced         -= OnItemPlaced;
            manager.OnItemPickedUp       -= OnItemPickedUp;
            manager.OnPlacementCancelled -= OnPlacementCancelled;
        }
    }
}
