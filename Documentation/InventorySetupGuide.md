# Inventory System Setup Guide

## Overview

The inventory system consists of four components:

| Script | Role |
|--------|------|
| `InventoryManager` | Stores items and quantities, fires change events |
| `InventorySlot` | Data model pairing a `PlaceableItem` with a quantity |
| `InventorySlotUI` | Single slot button in the UI grid |
| `InventoryUI` | Builds the full grid UI and bridges clicks to placement |
| `InventoryPlacementBridge` | Enforces quantity checks and deducts on placement |

---

## Step 1: Add Scripts to PlacementSystem GameObject

1. Select `PlacementSystem` in the Hierarchy
2. Click **Add Component** and add:
   - `InventoryManager`
   - `InventoryPlacementBridge`

---

## Step 2: Configure Starting Items in InventoryManager

1. Select `PlacementSystem`, find the **InventoryManager** component
2. Expand **Starting Items** and click the **+** button to add slots
3. For each slot:
   - Drag a `PlaceableItem` ScriptableObject into the **Item** field
   - Set the **Quantity** (e.g., 5)

---

## Step 3: Create the Inventory UI Canvas

1. Right-click in Hierarchy → `UI > Canvas`
2. Name it `InventoryCanvas`
3. Set **Canvas Scaler** → UI Scale Mode: `Scale With Screen Size`

### Create the Panel

1. Right-click `InventoryCanvas` → `UI > Panel`
2. Name it `InventoryPanel`
3. Anchor it to the bottom of the screen (use the Anchor Presets in the Inspector)
4. Set a comfortable height (e.g., 150px)

### Create the Slot Container

1. Right-click `InventoryPanel` → `UI > Scroll View` (optional) or `UI > Empty`
2. Name it `SlotContainer`
3. Add a **Horizontal Layout Group** component
   - Spacing: 8
   - Child Alignment: Middle Left
   - Control Child Size: Width ✅, Height ✅

### Create the Selected Item Label

1. Right-click `InventoryPanel` → `UI > Text - TextMeshPro`
2. Name it `SelectedItemText`
3. Position it above the slot container

### Create the Deselect Button

1. Right-click `InventoryPanel` → `UI > Button - TextMeshPro`
2. Name it `DeselectButton`
3. Set button text to `Cancel`

---

## Step 4: Create the Slot Prefab

1. Right-click `SlotContainer` → `UI > Button - TextMeshPro`
2. Name it `InventorySlotPrefab`
3. Add the **InventorySlotUI** component to it
4. Inside the button, add:
   - An **Image** child named `Icon` — assign to `iconImage` field
   - A **TextMeshPro** child named `ItemName` — assign to `itemNameText` field
   - A **TextMeshPro** child named `Quantity` — assign to `quantityText` field
5. Drag `InventorySlotPrefab` from the Hierarchy into the **Project panel** to make it a prefab
6. Delete it from the Hierarchy

---

## Step 5: Add and Configure InventoryUI

1. Select `InventoryPanel`
2. Add the **InventoryUI** component
3. Assign the fields:
   - **Inventory Manager** → drag `PlacementSystem`
   - **Placement Controller** → drag `PlacementSystem`
   - **Slot Container** → drag `SlotContainer`
   - **Slot Prefab** → drag the `InventorySlotPrefab` prefab from the Project panel
   - **Selected Item Text** → drag `SelectedItemText`
   - **Deselect Button** → drag `DeselectButton`

---

## Step 6: Test

1. Press **Play**
2. The inventory panel should show buttons for each item in your starting items list
3. Click an item button — the green preview should appear
4. Place items — the quantity counter should decrease
5. When quantity reaches 0, the slot greys out and placement is automatically cancelled

---

## Architecture Notes

- `InventoryManager` is pure logic — no UI dependency
- `InventoryUI` only reads from `InventoryManager` via events — no direct data mutation
- `InventoryPlacementBridge` is the only component that connects placement to inventory deduction
- To add items at runtime: `inventoryManager.AddItem(item, quantity)`
- To check stock: `inventoryManager.HasItem(itemId)`
