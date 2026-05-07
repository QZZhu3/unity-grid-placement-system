using UnityEngine;
using System.Collections;

/// <summary>
/// Test driver for the inventory-integrated placement system.
/// - Auto-starts placement on Play
/// - After each successful placement, immediately starts the next one (if quantity > 0)
/// - When a placed item is picked up, it re-enters drag mode automatically
/// </summary>
public class DragTest : MonoBehaviour
{
    public PlaceableItem testItem;

    private PlacementManager manager;
    private InventoryManager inventory;

    void Start()
    {
        manager   = GetComponent<PlacementManager>();
        inventory = FindAnyObjectByType<InventoryManager>();

        // Subscribe to events
        manager.OnItemPlaced         += OnItemPlaced;
        manager.OnItemPickedUp       += OnItemPickedUp;
        manager.OnPlacementCancelled += OnPlacementCancelled;

        // Auto-start first placement
        TryBeginPlacement();
    }

    private void OnItemPlaced(PlacedItem item)
    {
        // After placing, immediately start the next placement if we still have stock
        StartCoroutine(BeginNextFrame());
    }

    private void OnItemPickedUp(PlacedItem item)
    {
        // Item is now being dragged again — PlacementManager handles this automatically.
        // Nothing extra needed here; the DraggableItem is already active.
    }

    private void OnPlacementCancelled(PlacedItem item)
    {
        // Pick-up was cancelled — item went back to the grid.
        // Start a fresh placement if we still have stock.
        StartCoroutine(BeginNextFrame());
    }

    private IEnumerator BeginNextFrame()
    {
        // Wait one frame so PlacementManager finishes cleanup before we call BeginPlacement again
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
