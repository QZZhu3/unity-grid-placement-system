using UnityEngine;

/// <summary>
/// Bridges the InventoryManager and PlacementController.
/// Enforces quantity checks before placement is allowed and
/// automatically deselects items when stock reaches zero.
/// Attach this to the same GameObject as PlacementController and InventoryManager.
/// </summary>
public class InventoryPlacementBridge : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlacementController placementController;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (placementController == null)
            placementController = FindAnyObjectByType<PlacementController>();

        // Hook into placement validation
        placementController.OnPlacementValidation += HandlePlacementValidation;

        // Hook into successful placement to deduct quantity
        placementController.OnItemPlaced += HandleItemPlaced;
    }

    /// <summary>
    /// Called every frame when the placement controller checks if placement is valid.
    /// Returns false (blocking placement) if the item has 0 quantity in inventory.
    /// </summary>
    private bool HandlePlacementValidation(PlaceableItem item)
    {
        if (item == null) return false;
        return inventoryManager.HasItem(item.ItemId, 1);
    }

    /// <summary>
    /// Called after an item is successfully placed.
    /// Removes one unit from the inventory.
    /// </summary>
    private void HandleItemPlaced(PlacedItem placedItem)
    {
        inventoryManager.RemoveItem(placedItem.ItemId, 1);

        // If stock is now zero, force deselect
        if (!inventoryManager.HasItem(placedItem.ItemId, 1))
        {
            placementController.DeselectItem();
            Debug.Log($"[InventoryPlacementBridge] {placedItem.ItemId} is out of stock. Deselecting.");
        }
    }

    private void OnDestroy()
    {
        if (placementController != null)
        {
            placementController.OnPlacementValidation -= HandlePlacementValidation;
            placementController.OnItemPlaced -= HandleItemPlaced;
        }
    }
}
