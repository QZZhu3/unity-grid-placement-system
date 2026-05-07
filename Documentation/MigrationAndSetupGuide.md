# Migration & Scene Setup Guide — Draggable Placement Architecture

## What Changed

| Old System | New System | Notes |
|------------|------------|-------|
| `PlacementController` | `PlacementManager` | Orchestrates flow; no direct input reads |
| `PlacementController` (input) | `MouseInputController` | All input isolated here |
| *(none)* | `IInputController` | Interface for future touch support |
| *(none)* | `PlacementValidator` | Stateless validation, replaces inline checks |
| *(none)* | `DraggableItem` | Runtime component added during drag |
| `GridManager` | `GridManager` (refactored) | Same API, cleaner bounds/gizmos |
| `PlacementPreview` | `PlacementPreview` (unchanged) | Green/red material feedback |
| `PlaceableItem` | `PlaceableItem` (unchanged) | ScriptableObject, no changes needed |

---

## Migration Steps

### Step 1 — Remove old scripts from your scene

1. Select `PlacementSystem` in the Hierarchy
2. Remove the **PlacementController** component (click the three-dot menu → Remove Component)
3. Keep **GridManager** — it has been updated in place

### Step 2 — Pull the new scripts

```
cd "C:\Users\Nz\Documents\UnityProjects\ProductivityTrackerGarden\Assets\PlacementSystem"
git pull
```

Unity will recompile. Expect a brief error until Step 3 is complete.

### Step 3 — Add new components to PlacementSystem

Select `PlacementSystem` in the Hierarchy and add these components:

1. **MouseInputController**
2. **PlacementValidator**
3. **PlacementManager**

### Step 4 — Configure MouseInputController

| Field | Value |
|-------|-------|
| Main Camera | Drag `Main Camera` from Hierarchy |
| Ground Layer Mask | Select `Ground` only |
| Raycast Distance | `1000` |

### Step 5 — Verify PlacementManager auto-wires

PlacementManager uses `FindAnyObjectByType` to locate GridManager, PlacementValidator, and MouseInputController automatically. No manual wiring needed unless you have multiple instances.

### Step 6 — Update any inventory integration

If you have `InventoryUI` calling `placementController.SelectItem(item)`, update it to call:

```csharp
placementManager.BeginPlacement(item);
```

Replace the `[SerializeField] private PlacementController` reference with:

```csharp
[SerializeField] private PlacementManager placementManager;
```

---

## Scene Setup (Fresh Project)

### Required GameObjects

```
Scene
├── Main Camera          (tag: MainCamera)
├── Directional Light
├── Ground               (3D Plane, Layer: Ground, has MeshCollider)
└── PlacementSystem      (Empty GameObject)
    ├── GridManager
    ├── MouseInputController
    ├── PlacementValidator
    └── PlacementManager
```

### Ground Layer

1. Select the `Ground` plane
2. In the Inspector, click **Layer → Add Layer**
3. Add `Ground` to an empty slot
4. Assign the `Ground` layer to the plane
5. Set **MouseInputController → Ground Layer Mask** to `Ground`

### GridManager Settings

| Field | Recommended Value |
|-------|------------------|
| Grid Width | 20 |
| Grid Height | 20 |
| Cell Size | 1 |
| Grid Origin | X: -10, Y: 0, Z: -10 |

---

## How to Start Placement

### From a script (e.g., InventoryUI button click):

```csharp
[SerializeField] private PlacementManager placementManager;
[SerializeField] private PlaceableItem    itemToPlace;

void OnItemButtonClicked()
{
    placementManager.BeginPlacement(itemToPlace);
}
```

### Picking up an existing object:

This happens automatically. When the player left-clicks on a placed object while nothing is being dragged, `PlacementManager` detects the hit and calls `PickUp()` internally.

---

## Controls

| Action | Input |
|--------|-------|
| Drag item | Move mouse |
| Confirm placement | Left-click (when preview is green) |
| Cancel placement | Right-click or Escape |
| Rotate 90° | R key |
| Pick up placed object | Left-click on it |

---

## Adding Touch Support (Future)

1. Create a new class `TouchInputController : MonoBehaviour, IInputController`
2. Implement all interface members using `Touchscreen.current`
3. In the scene, swap `MouseInputController` for `TouchInputController`
4. No other scripts need to change — they all depend on `IInputController`

---

## Architecture Rules (Keep These)

- `DraggableItem` **never** reads `Mouse.current` or `Input.*` directly
- `PlacementManager` **never** calls `occupancyGrid[x,z]` directly — always via `GridManager`
- `PlacementValidator` is **stateless** — no fields, no MonoBehaviour state between calls
- `GridManager` has **no knowledge** of UI, dragging, input, or inventory
