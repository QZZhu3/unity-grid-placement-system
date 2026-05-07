using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the inventory UI panel, dynamically building a grid of slot buttons
/// from the InventoryManager data. Bridges UI interaction with PlacementManager.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlacementManager placementManager;

    [Header("UI Elements")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI selectedItemText;

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private InventorySlotUI currentlySelectedSlotUI;
    private PlaceableItem currentSelectedItemData;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (placementManager == null)
            placementManager = FindAnyObjectByType<PlacementManager>();

        // Subscribe to inventory events
        inventoryManager.OnInventoryChanged += HandleInventoryChanged;
        inventoryManager.OnInventoryRefreshed += BuildSlots;

        // Subscribe to placement events to handle deselect on pick-up or placement end
        placementManager.OnItemPlaced += HandleItemPlaced;
        placementManager.OnItemPickedUp += HandleItemPickedUp;
        placementManager.OnPlacementCancelled += HandlePlacementCancelled;

        BuildSlots();
    }

    /// <summary>
    /// Clears and rebuilds all slot UI elements from current inventory data.
    /// </summary>
    private void BuildSlots()
    {
        // Clear existing slots
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        slotUIs.Clear();
        currentlySelectedSlotUI = null;

        // Build new slots
        List<InventorySlot> slots = inventoryManager.GetAllSlots();
        foreach (InventorySlot slot in slots)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(slot, OnSlotClicked);
                slotUIs.Add(slotUI);
            }
        }

        UpdateSelectedItemText();
    }

    /// <summary>
    /// Called when a slot is clicked — selects the item and starts placement.
    /// </summary>
    private void OnSlotClicked(InventorySlot slot)
    {
        if (slot.IsEmpty) return;
        
        // If already dragging something, ignore or cancel first
        if (placementManager.IsDragging) return;

        // Deselect previous
        if (currentlySelectedSlotUI != null)
            currentlySelectedSlotUI.SetSelected(false);

        // Find and select the new slot UI
        currentlySelectedSlotUI = slotUIs.Find(s => s.Slot == slot);
        if (currentlySelectedSlotUI != null)
            currentlySelectedSlotUI.SetSelected(true);

        currentSelectedItemData = slot.Item;

        // Tell PlacementManager to start placing this item
        placementManager.BeginPlacement(slot.Item);
        UpdateSelectedItemText();
    }

    /// <summary>
    /// Deselects the current item.
    /// </summary>
    public void DeselectItem()
    {
        if (currentlySelectedSlotUI != null)
        {
            currentlySelectedSlotUI.SetSelected(false);
            currentlySelectedSlotUI = null;
        }
        
        currentSelectedItemData = null;
        UpdateSelectedItemText();
    }

    /// <summary>
    /// Called when an item is placed.
    /// </summary>
    private void HandleItemPlaced(PlacedItem placedItem)
    {
        // If we were placing a new item from inventory, check if we need to deselect
        if (currentSelectedItemData != null && currentSelectedItemData.ItemId == placedItem.ItemId)
        {
            // If quantity reached 0, deselect automatically
            if (!inventoryManager.HasItem(placedItem.ItemId))
            {
                DeselectItem();
            }
            else
            {
                // Continue placing the same item type if we still have some left
                // Update the text to reflect the new quantity
                UpdateSelectedItemText();
                
                // Wait for the next frame to begin placement again to avoid input conflicts
                StartCoroutine(BeginPlacementNextFrame(currentSelectedItemData));
            }
        }
        else
        {
            DeselectItem();
        }
    }
    
    private System.Collections.IEnumerator BeginPlacementNextFrame(PlaceableItem itemData)
    {
        yield return null;
        if (inventoryManager.HasItem(itemData.ItemId))
        {
            placementManager.BeginPlacement(itemData);
        }
    }

    private void HandleItemPickedUp(PlacedItem placedItem)
    {
        // When picking up an item, clear selection from inventory UI
        DeselectItem();
    }

    private void HandlePlacementCancelled(PlacedItem placedItem)
    {
        // When placement is cancelled, clear selection
        DeselectItem();
    }

    /// <summary>
    /// Called when inventory data changes — refreshes the affected slot UI.
    /// </summary>
    private void HandleInventoryChanged(string itemId, int newQuantity)
    {
        InventorySlotUI slotUI = slotUIs.Find(s => s.Slot?.Item?.ItemId == itemId);
        slotUI?.Refresh();
        
        if (currentSelectedItemData != null && currentSelectedItemData.ItemId == itemId)
        {
            UpdateSelectedItemText();
        }
    }

    /// <summary>
    /// Updates the selected item label.
    /// </summary>
    private void UpdateSelectedItemText()
    {
        if (selectedItemText == null) return;

        if (currentSelectedItemData == null)
        {
            selectedItemText.text = "No item selected";
        }
        else
        {
            int qty = inventoryManager.GetQuantity(currentSelectedItemData.ItemId);
            selectedItemText.text = $"Placing: {currentSelectedItemData.DisplayName}  ({qty} left)";
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= HandleInventoryChanged;
            inventoryManager.OnInventoryRefreshed -= BuildSlots;
        }

        if (placementManager != null)
        {
            placementManager.OnItemPlaced -= HandleItemPlaced;
            placementManager.OnItemPickedUp -= HandleItemPickedUp;
            placementManager.OnPlacementCancelled -= HandlePlacementCancelled;
        }
    }
}
