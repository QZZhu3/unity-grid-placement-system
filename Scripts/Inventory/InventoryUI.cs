using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the inventory UI panel.
///
/// Responsibilities:
///   - Builds a scrollable grid of <see cref="InventorySlotUI"/> buttons from InventoryManager data.
///   - Clicking a slot with qty > 0 calls PlacementManager.BeginPlacement().
///   - Automatically deselects when a drag ends for any reason (place, cancel, basket return).
///   - Re-starts placement of the same item type after a successful place, as long as qty > 0.
///   - Rebuilds the entire slot list when InventoryManager fires OnInventoryRefreshed (e.g. after load).
///
/// Architecture rules:
///   - Never reads or writes inventory data directly — only via InventoryManager.
///   - Never calls PlacementManager internals — only BeginPlacement() and IsDragging.
///   - All coupling is through events; no polling in Update().
/// </summary>
public class InventoryUI : MonoBehaviour
{
    // ── Inspector references ──────────────────────────────────────────────────

    [Header("Core References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlacementManager placementManager;

    [Header("UI Elements")]
    [Tooltip("Parent transform that holds the slot buttons (e.g. a Grid Layout Group).")]
    [SerializeField] private Transform slotContainer;

    [Tooltip("Prefab with an InventorySlotUI component.")]
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("Optional label showing the currently selected item and remaining quantity.")]
    [SerializeField] private TextMeshProUGUI selectedItemText;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private readonly List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private InventorySlotUI  selectedSlotUI;
    private PlaceableItem    selectedItemData;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindAnyObjectByType<InventoryManager>();

        if (placementManager == null)
            placementManager = FindAnyObjectByType<PlacementManager>();

        // Inventory data events
        inventoryManager.OnInventoryChanged   += HandleInventoryChanged;
        inventoryManager.OnInventoryRefreshed += BuildSlots;

        // Placement lifecycle events
        placementManager.OnItemPlaced         += HandleItemPlaced;
        placementManager.OnItemPickedUp       += HandleItemPickedUp;
        placementManager.OnPlacementCancelled += HandlePlacementCancelled;
        placementManager.OnDragEnded          += HandleDragEnded;   // covers basket return

        BuildSlots();
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged   -= HandleInventoryChanged;
            inventoryManager.OnInventoryRefreshed -= BuildSlots;
        }

        if (placementManager != null)
        {
            placementManager.OnItemPlaced         -= HandleItemPlaced;
            placementManager.OnItemPickedUp       -= HandleItemPickedUp;
            placementManager.OnPlacementCancelled -= HandlePlacementCancelled;
            placementManager.OnDragEnded          -= HandleDragEnded;
        }
    }

    // ── Slot construction ─────────────────────────────────────────────────────

    /// <summary>
    /// Destroys all existing slot buttons and rebuilds them from the current inventory state.
    /// Called on Start and whenever InventoryManager fires OnInventoryRefreshed (e.g. after load).
    /// </summary>
    private void BuildSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        slotUIs.Clear();
        selectedSlotUI   = null;
        selectedItemData = null;

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

    // ── Slot interaction ──────────────────────────────────────────────────────

    /// <summary>
    /// Called when the player clicks/taps a slot button.
    /// Starts placement if the slot is non-empty and no drag is currently active.
    /// </summary>
    private void OnSlotClicked(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty) return;

        // Ignore taps while already dragging (prevents double-spawn)
        if (placementManager.IsDragging) return;

        // Deselect the previously selected slot
        if (selectedSlotUI != null)
            selectedSlotUI.SetSelected(false);

        // Select the new slot
        selectedSlotUI  = slotUIs.Find(s => s.Slot == slot);
        selectedItemData = slot.Item;

        if (selectedSlotUI != null)
            selectedSlotUI.SetSelected(true);

        placementManager.BeginPlacement(slot.Item);
        UpdateSelectedItemText();
    }

    // ── Placement event handlers ──────────────────────────────────────────────

    /// <summary>
    /// After a successful placement, either chain the next placement (if qty > 0)
    /// or deselect the slot (if we just used the last item).
    /// </summary>
    private void HandleItemPlaced(PlacedItem placedItem)
    {
        if (selectedItemData != null && selectedItemData.ItemId == placedItem.ItemId)
        {
            if (inventoryManager.HasItem(placedItem.ItemId))
            {
                // Still have stock — chain the next placement after two frames
                // (one for PlacementManager cleanup, one for the cooldown to clear)
                UpdateSelectedItemText();
                StartCoroutine(BeginPlacementNextFrame(selectedItemData));
            }
            else
            {
                // Ran out of stock — deselect
                DeselectItem();
            }
        }
        else
        {
            // A picked-up item was re-placed; clear any stale selection
            DeselectItem();
        }
    }

    /// <summary>
    /// When the player picks up a world item, clear the inventory selection.
    /// The pick-up drag is managed entirely by PlacementManager.
    /// </summary>
    private void HandleItemPickedUp(PlacedItem placedItem)
    {
        DeselectItem();
    }

    /// <summary>
    /// When a pick-up drag is cancelled (ESC / programmatic), the item returns to the grid.
    /// Clear the inventory selection.
    /// </summary>
    private void HandlePlacementCancelled(PlacedItem placedItem)
    {
        DeselectItem();
    }

    /// <summary>
    /// Fired by PlacementManager whenever any drag ends — including basket returns.
    /// This is the catch-all deselect for cases not covered by the above handlers.
    /// </summary>
    private void HandleDragEnded(DraggableItem draggable)
    {
        // Only deselect if we are not about to chain another placement.
        // The chain coroutine from HandleItemPlaced will have already been queued
        // if applicable, so we only need to deselect here for non-chain endings
        // (basket return, cancel of a new item, etc.).
        if (!placementManager.IsDragging)
        {
            // If selectedItemData is still set but we are not chaining, clear it.
            // The coroutine sets selectedItemData before calling BeginPlacement,
            // so if IsDragging is false here and no coroutine is running, we're done.
            // We use a one-frame delay so the chain coroutine (if any) runs first.
            StartCoroutine(DeferredDeselectIfIdle());
        }
    }

    /// <summary>
    /// Waits one frame, then deselects if no drag has started in the meantime.
    /// This prevents HandleDragEnded from cancelling a chain placement that is
    /// about to begin on the next frame.
    /// </summary>
    private IEnumerator DeferredDeselectIfIdle()
    {
        yield return null;
        if (!placementManager.IsDragging)
            DeselectItem();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Waits two frames then begins placement of the same item type.
    /// Guards against qty=0 and an already-active drag.
    /// </summary>
    private IEnumerator BeginPlacementNextFrame(PlaceableItem itemData)
    {
        yield return null;
        yield return null;

        // Guard: inventory may have changed during the two-frame wait
        if (itemData == null) yield break;
        if (!inventoryManager.HasItem(itemData.ItemId)) { DeselectItem(); yield break; }
        if (placementManager.IsDragging) yield break;

        placementManager.BeginPlacement(itemData);
        UpdateSelectedItemText();
    }

    /// <summary>
    /// Clears the current selection visually and in state.
    /// Safe to call when nothing is selected.
    /// </summary>
    public void DeselectItem()
    {
        if (selectedSlotUI != null)
        {
            selectedSlotUI.SetSelected(false);
            selectedSlotUI = null;
        }

        selectedItemData = null;
        UpdateSelectedItemText();
    }

    // ── Inventory event handlers ──────────────────────────────────────────────

    /// <summary>
    /// Called when a single item's quantity changes.
    /// Refreshes only the affected slot to avoid a full rebuild.
    /// </summary>
    private void HandleInventoryChanged(string itemId, int newQuantity)
    {
        InventorySlotUI slotUI = slotUIs.Find(s => s.Slot?.Item?.ItemId == itemId);
        slotUI?.Refresh();

        if (selectedItemData != null && selectedItemData.ItemId == itemId)
            UpdateSelectedItemText();
    }

    /// <summary>
    /// Updates the status label with the selected item name and remaining quantity.
    /// </summary>
    private void UpdateSelectedItemText()
    {
        if (selectedItemText == null) return;

        if (selectedItemData == null)
        {
            selectedItemText.text = "Select an item to place";
        }
        else
        {
            int qty = inventoryManager.GetQuantity(selectedItemData.ItemId);
            selectedItemText.text = $"Placing: {selectedItemData.DisplayName}  ({qty} left)";
        }
    }
}
