using UnityEngine;

/// <summary>
/// Connects the pure-data InventoryManager to the PlacementManager.
/// Handles the anti-duplication logic:
/// - New placement: -1 from inventory
/// - Pick up existing: +1 to inventory temporarily
/// - Re-place existing: -1 from inventory (net zero)
/// - Cancel pick up: -1 from inventory (net zero)
/// </summary>
public class InventoryPlacementBridge : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlacementManager placementManager;

    private void Start()
    {
        if (inventoryManager == null) inventoryManager = FindAnyObjectByType<InventoryManager>();
        if (placementManager == null) placementManager = FindAnyObjectByType<PlacementManager>();

        placementManager.OnItemPlaced += HandleItemPlaced;
        placementManager.OnItemPickedUp += HandleItemPickedUp;
        placementManager.OnPlacementCancelled += HandlePlacementCancelled;
    }

    private void HandleItemPlaced(PlacedItem item)
    {
        // Whether it's a new item or a re-placed item, we deduct 1.
        // If it was a new item, we are deducting it for the first time.
        // If it was a picked-up item, we added 1 back during pick-up, so we deduct 1 now to net zero.
        inventoryManager.RemoveItem(item.ItemId, 1);
    }

    private void HandleItemPickedUp(PlacedItem item)
    {
        // "Picking up an object returns it to inventory temporarily while dragging"
        // Add 1 to inventory.
        
        PlaceableItem itemData = GetItemData(item.ItemId);
        if (itemData != null)
        {
            inventoryManager.AddItem(itemData, 1);
        }
        else
        {
            Debug.LogWarning($"[InventoryPlacementBridge] Could not find PlaceableItem for id '{item.ItemId}' to return to inventory.");
        }
    }

    private void HandlePlacementCancelled(PlacedItem item)
    {
        // If we picked it up, we added 1. If we cancel, it goes back to the grid.
        // So we must DEDUCT 1 to net zero, because it's no longer in the inventory, it's on the grid again.
        inventoryManager.RemoveItem(item.ItemId, 1);
    }

    private PlaceableItem GetItemData(string itemId)
    {
        // Fallback to find the PlaceableItem by ID
        PlaceableItem[] all = Resources.FindObjectsOfTypeAll<PlaceableItem>();
        foreach (var item in all)
        {
            if (item.ItemId == itemId) return item;
        }
        return null;
    }

    private void OnDestroy()
    {
        if (placementManager != null)
        {
            placementManager.OnItemPlaced -= HandleItemPlaced;
            placementManager.OnItemPickedUp -= HandleItemPickedUp;
            placementManager.OnPlacementCancelled -= HandlePlacementCancelled;
        }
    }
}
