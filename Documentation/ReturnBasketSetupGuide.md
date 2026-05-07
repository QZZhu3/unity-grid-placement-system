# Return Basket System Setup Guide

## Overview

The Return Basket system allows players to return draggable objects back to their inventory by dragging and clicking over a designated UI zone. It seamlessly integrates with the `PlacementManager` and `InventoryManager` without introducing duplication bugs.

### Core Components
- **ReturnBasketZone**: A UI component that detects pointer hover states and signals the `PlacementManager`. It automatically shows/hides itself based on drag state.
- **PlacementManager Updates**: Now fires `OnDragStarted` and `OnDragEnded` events, and intercepts placement confirmation if hovering over the return basket.

---

## Scene Setup Instructions

### 1. Create the UI Basket
1. In your `InventoryCanvas` (or main UI Canvas), right-click and create a new **UI > Panel**. Name it `ReturnBasketZone`.
2. Anchor it to the bottom-left or bottom-right of the screen (e.g., Anchor Preset: Bottom-Left, Pos X: 100, Pos Y: 100, Width: 150, Height: 150).
3. Add a **Canvas Group** component to `ReturnBasketZone`.

### 2. Create the Visuals
1. Inside `ReturnBasketZone`, create an empty child GameObject named `Visuals`.
2. Inside `Visuals`, add an **Image** or **Text** to represent the basket (e.g., an icon or the word "Return").
3. Make sure the `ReturnBasketZone` itself has an Image component (can be fully transparent) with **Raycast Target** enabled so it catches mouse hover events.
4. Ensure the `Visuals` child has **Raycast Target** *disabled* so it doesn't block the parent's hover detection.

### 3. Attach the Script
1. Attach the `ReturnBasketZone.cs` script to the `ReturnBasketZone` GameObject.
2. In the Inspector, assign the references:
   - **Visual Container**: Drag the `Visuals` child object here.
   - **Canvas Group**: Drag the `ReturnBasketZone` itself here.
3. You can tweak the Hover Alpha and Hover Scale values to your liking.

---

## How It Works (Data Flow)

The anti-duplication logic handles returning items perfectly without needing extra code in the `InventoryPlacementBridge`:

### Scenario A: Returning a Brand New Item
1. Player clicks inventory slot. `BeginPlacement()` starts.
2. `PlacementManager` fires `OnDragStarted`. The basket becomes visible.
3. Player hovers over the basket. `ReturnBasketZone` calls `SetHoveringReturnBasket(true)`.
4. Player clicks to confirm. `PlacementManager` intercepts it.
5. `ReturnToInventory()` is called. Since it's a new item, it simply `Destroy()`s the preview.
6. **Inventory result:** Net zero change. The item was never officially placed, so it was never deducted.

### Scenario B: Returning an Existing (Placed) Item
1. Player clicks a placed item on the grid. `PickUp()` starts.
2. `InventoryPlacementBridge` receives `OnItemPickedUp` and **adds +1** to the inventory.
3. The item's grid cells are freed.
4. `PlacementManager` fires `OnDragStarted`. The basket becomes visible.
5. Player hovers over the basket and clicks.
6. `ReturnToInventory()` is called. It `Destroy()`s the item permanently.
7. **Inventory result:** Net +1 change. The item was removed from the world and the +1 from the pick-up remains in the inventory. No duplicate is created, and no items are lost.

### Why not just use `OnPlacementCancelled`?
If we fired `OnPlacementCancelled` when returning to the basket, the `InventoryPlacementBridge` would deduct -1 from the inventory (which is the correct behaviour for pressing Escape, returning the item to the grid). By bypassing `OnPlacementCancelled` and simply destroying the object, we effectively "keep" the +1 we got during pick-up, cleanly returning the item to storage.
