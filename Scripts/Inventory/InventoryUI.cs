using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the inventory UI panel, dynamically building a grid of slot buttons
/// from the InventoryManager data. Bridges UI interaction with PlacementController.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlacementController placementController;

    [Header("UI Elements")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private TextMeshProUGUI selectedItemText;
    [SerializeField] private Button deselectButton;

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private InventorySlotUI currentlySelectedSlotUI;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (placementController == null)
            placementController = FindAnyObjectByType<PlacementController>();

        // Subscribe to inventory events
        inventoryManager.OnInventoryChanged += HandleInventoryChanged;
        inventoryManager.OnInventoryRefreshed += BuildSlots;

        // Subscribe to placement events to reduce quantity
        placementController.OnItemPlaced += HandleItemPlaced;

        if (deselectButton != null)
            deselectButton.onClick.AddListener(DeselectItem);

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

        // Deselect previous
        if (currentlySelectedSlotUI != null)
            currentlySelectedSlotUI.SetSelected(false);

        // Find and select the new slot UI
        currentlySelectedSlotUI = slotUIs.Find(s => s.Slot == slot);
        if (currentlySelectedSlotUI != null)
            currentlySelectedSlotUI.SetSelected(true);

        // Tell PlacementController to start placing this item
        placementController.SelectItem(slot.Item);
        UpdateSelectedItemText();
    }

    /// <summary>
    /// Deselects the current item and cancels placement.
    /// </summary>
    public void DeselectItem()
    {
        if (currentlySelectedSlotUI != null)
        {
            currentlySelectedSlotUI.SetSelected(false);
            currentlySelectedSlotUI = null;
        }

        placementController.DeselectItem();
        UpdateSelectedItemText();
    }

    /// <summary>
    /// Called when an item is placed — removes one from inventory.
    /// </summary>
    private void HandleItemPlaced(PlacedItem placedItem)
    {
        inventoryManager.RemoveItem(placedItem.ItemId, 1);

        // If quantity reached 0, deselect automatically
        if (!inventoryManager.HasItem(placedItem.ItemId))
        {
            DeselectItem();
        }
    }

    /// <summary>
    /// Called when inventory data changes — refreshes the affected slot UI.
    /// </summary>
    private void HandleInventoryChanged(string itemId, int newQuantity)
    {
        InventorySlotUI slotUI = slotUIs.Find(s => s.Slot?.Item?.ItemId == itemId);
        slotUI?.Refresh();
    }

    /// <summary>
    /// Updates the selected item label.
    /// </summary>
    private void UpdateSelectedItemText()
    {
        if (selectedItemText == null) return;

        PlaceableItem selected = placementController.GetSelectedItem();
        if (selected == null)
        {
            selectedItemText.text = "No item selected";
        }
        else
        {
            int qty = inventoryManager.GetQuantity(selected.ItemId);
            selectedItemText.text = $"Placing: {selected.DisplayName}  ({qty} left)";
        }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= HandleInventoryChanged;
            inventoryManager.OnInventoryRefreshed -= BuildSlots;
        }

        if (placementController != null)
            placementController.OnItemPlaced -= HandleItemPlaced;
    }
}
