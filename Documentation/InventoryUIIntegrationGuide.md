# Inventory UI Integration Guide

This document explains how to wire up the inventory-driven placement system in the Unity scene, describes the complete gameplay flow, and lists all deprecated components to remove.

---

## Overview

The inventory-driven placement system replaces the old `DragTest` auto-spawn prototype. Players now select items by clicking or tapping a slot in the **Inventory UI** panel. The system is fully event-driven and split into three independent layers:

| Layer | Script | Responsibility |
|---|---|---|
| Data | `InventoryManager` | Tracks item quantities; fires events on change |
| Bridge | `InventoryPlacementBridge` | Keeps inventory counts correct across all placement scenarios |
| UI | `InventoryUI` + `InventorySlotUI` | Displays slots; calls `PlacementManager.BeginPlacement()` on tap |

---

## Scene Setup

### 1. Core Gameplay Objects (already in scene)

Ensure the following components exist on the `PlacementSystem` GameObject:

- `PlacementManager`
- `MouseInputController`
- `PlacementValidator`
- `InventoryManager` — with `Starting Items` list populated in the Inspector
- `InventoryPlacementBridge`
- `SaveManager`

Remove or disable these legacy components if they are still present:

- `DragTest` — now a no-op stub; remove from the scene
- `PlacementController` — deprecated; remove from the scene
- `PlacementUIManager` — deprecated; remove from the scene

---

### 2. Inventory UI Canvas

Create the following hierarchy under your main Canvas:

```
Canvas
└── InventoryPanel          (Panel, anchored bottom-left or bottom-centre)
    ├── ScrollView           (Scroll Rect component)
    │   └── Viewport
    │       └── SlotContainer  (Grid Layout Group, cell size ≥ 80×80 px)
    └── SelectedItemLabel    (TextMeshProUGUI, optional status text)
```

Add the `InventoryUI` component to `InventoryPanel` and wire up its Inspector fields:

| Field | Value |
|---|---|
| Inventory Manager | drag `PlacementSystem` (has `InventoryManager`) |
| Placement Manager | drag `PlacementSystem` (has `PlacementManager`) |
| Slot Container | drag `SlotContainer` |
| Slot Prefab | drag the `InventorySlotPrefab` (see below) |
| Selected Item Text | drag `SelectedItemLabel` (optional) |

---

### 3. Inventory Slot Prefab

Create a prefab at `Assets/Prefabs/UI/InventorySlotPrefab.prefab` with this hierarchy:

```
InventorySlotPrefab         (RectTransform, min 80×80 px)
├── Background              (Image — dark fill, receives colour changes)
├── Icon                    (Image — item icon, centred)
├── ItemName                (TextMeshProUGUI — item display name)
├── Quantity                (TextMeshProUGUI — item quantity, bottom-right corner)
└── SelectionHighlight      (Image — bright border, disabled by default)
```

Add the `InventorySlotUI` component to the root and wire its Inspector fields:

| Field | Value |
|---|---|
| Icon Image | drag `Icon` |
| Item Name Text | drag `ItemName` |
| Quantity Text | drag `Quantity` |
| Background Image | drag `Background` |
| Selection Highlight | drag `SelectionHighlight` |

**Mobile sizing note:** Set the `Grid Layout Group` cell size to at least `80×80` px and padding to `8` px. This ensures slots are comfortably tappable on small screens.

---

### 4. Return Basket Zone (already implemented)

The `ReturnBasketZone` component should be on a UI element in the Canvas. It shows automatically when a drag starts and hides when it ends. No additional wiring is needed for the inventory integration.

---

### 5. Save/Load (already implemented)

`SaveManager` is already wired. No changes are needed. The Inventory UI rebuilds automatically after a load because `InventoryManager.OnInventoryRefreshed` fires after `InventorySaveHandler.LoadFromSaveData()` runs.

---

## Gameplay Flow

### Placing a New Item

1. Player opens the Inventory UI panel.
2. Player taps a slot with quantity > 0.
3. `InventorySlotUI.OnPointerClick` fires → `InventoryUI.OnSlotClicked` is called.
4. `InventoryUI` calls `PlacementManager.BeginPlacement(item)`.
5. `PlacementManager` instantiates the prefab and attaches a `DraggableItem` component.
6. Player drags the item over the grid. `DraggableItem` handles snapping and validity preview.
7. Player left-clicks (or taps) on a valid cell → `DraggableItem` fires `OnConfirm`.
8. `PlacementManager.HandleConfirm` registers the item in `GridManager` and fires `OnItemPlaced`.
9. `InventoryPlacementBridge.HandleItemPlaced` calls `InventoryManager.RemoveItem` (−1).
10. `InventoryUI.HandleItemPlaced` checks remaining quantity:
    - **Qty > 0:** chains another `BeginPlacement` after two frames (continuous placement mode).
    - **Qty = 0:** calls `DeselectItem()` and updates the label.

### Cancelling a New Placement (ESC)

1. Player presses ESC while dragging a new item.
2. `DraggableItem` fires `OnCancel` → `PlacementManager.HandleCancel` destroys the preview.
3. `PlacementManager` fires `OnPlacementCancelled` (with `null` for new items — bridge ignores it).
4. `InventoryUI.HandlePlacementCancelled` calls `DeselectItem()`.
5. Inventory quantity is unchanged (deduction only happens on `OnItemPlaced`).

### Picking Up and Moving a Placed Item

1. Player left-clicks a placed object on the grid (no drag active).
2. `PlacementManager.TryPickUpAtCursor` finds the `PlacedItem` and calls `PickUp()`.
3. Grid cells are freed; `DraggableItem` is attached to the existing GameObject.
4. `PlacementManager` fires `OnItemPickedUp`.
5. `InventoryPlacementBridge.HandleItemPickedUp` calls `InventoryManager.AddItem` (+1 temporarily).
6. `InventoryUI.HandleItemPickedUp` calls `DeselectItem()` (no inventory slot should be selected during a pick-up drag).
7. Player drops the item on a new valid cell → same confirm flow as above (net zero inventory change).

### Cancelling a Pick-Up (ESC)

1. Player presses ESC while dragging a picked-up item.
2. `PlacementManager.HandleCancel` restores the item to its original grid position.
3. `PlacementManager` fires `OnPlacementCancelled` with the original `PlacedItem`.
4. `InventoryPlacementBridge.HandlePlacementCancelled` calls `InventoryManager.RemoveItem` (−1, netting the earlier +1 to zero).
5. `InventoryUI.HandlePlacementCancelled` calls `DeselectItem()`.

### Returning an Item to Inventory (Basket)

1. Player drags an item (new or picked-up) over the `ReturnBasketZone`.
2. `ReturnBasketZone` calls `PlacementManager.SetHoveringReturnBasket(true)`.
3. Player releases (left-click) → `PlacementManager` intercepts the confirm and calls `ReturnToInventory(draggable)`.
4. The preview/object is destroyed. No `OnPlacementCancelled` is fired.
5. `PlacementManager` fires `OnDragEnded`.
6. `InventoryUI.HandleDragEnded` runs a one-frame deferred deselect (to avoid racing with any chain coroutine).
7. Inventory quantity:
    - **New item:** unchanged (deduction never happened).
    - **Picked-up item:** +1 from pick-up remains, so the item is correctly back in inventory.

---

## Anti-Duplication Contract

The following table shows the net inventory change for every scenario. The bridge is the sole authority on inventory math; the UI never calls `AddItem` or `RemoveItem` directly.

| Scenario | Bridge action | Net Δ inventory |
|---|---|---|
| New item placed | `RemoveItem` on `OnItemPlaced` | −1 |
| New item cancelled (ESC) | none | 0 |
| New item returned to basket | none | 0 |
| Existing item picked up | `AddItem` on `OnItemPickedUp` | +1 (temporary) |
| Existing item re-placed | `RemoveItem` on `OnItemPlaced` | 0 net |
| Existing item pick-up cancelled | `RemoveItem` on `OnPlacementCancelled` | 0 net |
| Existing item returned to basket | none (keeps the +1 from pick-up) | +1 (item back in inventory) |

---

## Deprecated Components

The following components are no longer part of the gameplay loop. They have been replaced with no-op stubs that log a warning and self-disable. Remove them from the scene and delete the files when convenient.

| Component | Replacement |
|---|---|
| `DragTest` | `InventoryUI` (slot click drives placement) |
| `PlacementController` | `PlacementManager` + `MouseInputController` |
| `PlacementUIManager` | `InventoryUI` |

---

## Architecture Rules

These rules must be maintained as the project grows:

1. **`InventoryManager` has no UI or placement knowledge.** It only fires `OnInventoryChanged` and `OnInventoryRefreshed`.
2. **`PlacementManager` has no inventory knowledge.** It only fires placement lifecycle events.
3. **`InventoryPlacementBridge` is the only class that calls both `InventoryManager` and listens to `PlacementManager`.** It must never be bypassed.
4. **`InventoryUI` only calls `PlacementManager.BeginPlacement()` and reads `PlacementManager.IsDragging`.** It never calls `AddItem` or `RemoveItem`.
5. **All placement entry points go through `PlacementManager.BeginPlacement()`.** No other code should instantiate placement prefabs directly.
