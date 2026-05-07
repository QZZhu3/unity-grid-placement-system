# Inventory System Setup Guide

## Overview

The inventory system has been completely rewritten to integrate with the new `PlacementManager` + `DraggableItem` architecture. It consists of five components:

| Script | Role |
|--------|------|
| `InventoryManager` | Pure data layer. Stores items and quantities, fires change events. |
| `InventorySlot` | Data model pairing a `PlaceableItem` with a quantity. |
| `InventorySlotUI` | Single slot button in the UI grid. |
| `InventoryUI` | Builds the full grid UI, bridges clicks to `PlacementManager.BeginPlacement()`. |
| `InventoryPlacementBridge` | The crucial link that handles anti-duplication and deducts quantities on placement. |

---

## Step 1: Add Scripts to PlacementSystem GameObject

1. Select `PlacementSystem` in the Hierarchy.
2. Click **Add Component** and add:
   - `InventoryManager`
   - `InventoryPlacementBridge`

---

## Step 2: Configure Starting Items in InventoryManager

1. Select `PlacementSystem`, find the **InventoryManager** component.
2. Expand **Starting Items** and click the **+** button to add slots.
3. For each slot:
   - Drag a `PlaceableItem` ScriptableObject into the **Item** field.
   - Set the **Quantity** (e.g., 5).

---

## Step 3: Create the Inventory UI Canvas

1. Right-click in Hierarchy → `UI > Canvas`.
2. Name it `InventoryCanvas`.
3. Set **Canvas Scaler** → UI Scale Mode: `Scale With Screen Size`.

### Create the Panel

1. Right-click `InventoryCanvas` → `UI > Panel`.
2. Name it `InventoryPanel`.
3. Anchor it to the bottom of the screen (use the Anchor Presets in the Inspector).
4. Set a comfortable height (e.g., 150px).

### Create the Slot Container

1. Right-click `InventoryPanel` → `UI > Scroll View` (optional) or `UI > Empty`.
2. Name it `SlotContainer`.
3. Add a **Horizontal Layout Group** component:
   - Spacing: 8
   - Child Alignment: Middle Left
   - Control Child Size: Width ✅, Height ✅

### Create the Selected Item Label

1. Right-click `InventoryPanel` → `UI > Text - TextMeshPro`.
2. Name it `SelectedItemText`.
3. Position it above the slot container.

---

## Step 4: Create the Slot Prefab

1. Right-click `SlotContainer` → `UI > Button - TextMeshPro`.
2. Name it `InventorySlotPrefab`.
3. Add the **InventorySlotUI** component to it.
4. Inside the button, add:
   - An **Image** child named `Icon` — assign to `iconImage` field.
   - A **TextMeshPro** child named `ItemName` — assign to `itemNameText` field.
   - A **TextMeshPro** child named `Quantity` — assign to `quantityText` field.
   - An **Image** child named `SelectionHighlight` (e.g., a yellow border, disable it by default) — assign to `selectionHighlight` field.
5. Drag `InventorySlotPrefab` from the Hierarchy into the **Project panel** to make it a prefab.
6. Delete it from the Hierarchy.

---

## Step 5: Add and Configure InventoryUI

1. Select `InventoryPanel`.
2. Add the **InventoryUI** component.
3. Assign the fields:
   - **Inventory Manager** → drag `PlacementSystem`
   - **Placement Manager** → drag `PlacementSystem`
   - **Slot Container** → drag `SlotContainer`
   - **Slot Prefab** → drag the `InventorySlotPrefab` prefab from the Project panel
   - **Selected Item Text** → drag `SelectedItemText`

---

## Step 6: Test

1. Press **Play**.
2. The inventory panel should show buttons for each item in your starting items list.
3. Click an item button — the green preview should appear.
4. Place items — the quantity counter should decrease.
5. When quantity reaches 0, the slot greys out and placement is automatically cancelled.
6. Pick up an existing item — the inventory should temporarily regain 1 quantity. Re-placing it deducts it again. Cancelling the pick-up also deducts it back to normal.

---

## Architecture & Placement Flow

The inventory system is designed with strict separation of concerns to prevent tight coupling and item duplication bugs.

### Core Components
- **InventoryManager**: Pure data layer. Tracks item quantities and fires events (`OnInventoryChanged`). Knows nothing about UI or placement.
- **InventoryUI & InventorySlotUI**: View layer. Listens to `InventoryManager` events to update visual counts. Clicking a slot calls `PlacementManager.BeginPlacement()`.
- **PlacementManager**: The orchestrator. Handles dragging, rotating, grid snapping, and firing placement events. Knows nothing about inventory.
- **InventoryPlacementBridge**: The crucial link. Listens to `PlacementManager` events and updates `InventoryManager` quantities to prevent duplication.

### The Anti-Duplication Flow
The bridge handles inventory quantities based on player actions:

1. **Start New Placement**: Player clicks an item in the UI. `InventoryUI` checks if quantity > 0 and calls `BeginPlacement()`. No quantity is deducted yet.
2. **Confirm New Placement**: Player clicks the grid. `PlacementManager` fires `OnItemPlaced`. The bridge deducts `1` from inventory.
3. **Cancel New Placement**: Player presses Escape/Right-click. `PlacementManager` destroys the preview. No events are fired, so no inventory changes happen (net zero).
4. **Pick Up Existing Item**: Player clicks a placed item. `PlacementManager` fires `OnItemPickedUp`. The bridge **adds `1`** to inventory temporarily while dragging.
5. **Re-Place Existing Item**: Player clicks the grid. `PlacementManager` fires `OnItemPlaced`. The bridge **deducts `1`** from inventory (net zero from pick up).
6. **Cancel Pick Up**: Player presses Escape/Right-click. `PlacementManager` restores the item to its original spot and fires `OnPlacementCancelled`. The bridge **deducts `1`** from inventory (net zero from pick up).

This ensures that items are never duplicated or lost during the drag-and-drop process, even when moving existing items around the grid.
