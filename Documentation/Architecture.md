# Grid-Based Placement System - Architecture

This document explains the architecture and design decisions of the grid-based placement system.

## System Overview

The placement system is designed as a modular, extensible framework for grid-based item placement in Unity. It separates concerns into distinct components, each with a specific responsibility.

## Component Hierarchy

```
PlacementSystem (Empty GameObject)
├── GridManager (Component)
│   ├── Manages grid data structure
│   ├── Tracks occupancy
│   └── Provides coordinate conversion
├── PlacementController (Component)
│   ├── Handles user input
│   ├── Manages preview object
│   ├── Validates placement
│   └── Fires events
└── Canvas (UI)
    └── PlacementUIManager (Component)
        ├── Creates item buttons
        └── Displays status
```

## Core Components

### GridManager

**Responsibility:** Maintain the grid state and provide grid-related queries.

**Key Features:**
* Stores a 2D boolean array representing cell occupancy.
* Provides bidirectional conversion between world positions and grid coordinates.
* Validates placement areas before allowing placement.
* Tracks placed items for later retrieval or removal.

**Design Decisions:**
* Uses a simple 2D boolean array for occupancy tracking, providing O(1) lookup time.
* Stores placed items in a dictionary keyed by grid position for quick retrieval.
* Gizmo visualization helps debug grid alignment in the scene editor.

### PlacementController

**Responsibility:** Manage user interaction and placement workflow.

**Key Features:**
* Raycasts from the camera to detect mouse position on the ground.
* Snaps the preview to the nearest grid cell.
* Updates preview validity based on occupancy checks.
* Handles rotation input (R key).
* Fires events when items are placed or removed.

**Design Decisions:**
* Separates preview logic from placement logic, allowing for easy customization.
* Uses raycasting for ground detection, supporting any collider-based ground.
* Rotation is handled entirely in the controller, with size adjustments delegated to `RotationHandler`.

### PlaceableItem

**Responsibility:** Define item properties in a reusable, inspector-friendly format.

**Key Features:**
* Stores item metadata (ID, prefab, size, display name, icon).
* Implements `OnValidate()` to auto-generate IDs and enforce minimum size.
* Serializable as a ScriptableObject for easy creation in the editor.

**Design Decisions:**
* Uses ScriptableObject instead of a data class for better editor integration.
* Size is stored as `Vector2Int` to match grid coordinates.
* Display name and icon support UI integration without coupling to UI code.

### PlacementPreview

**Responsibility:** Provide visual feedback for placement validity.

**Key Features:**
* Dynamically creates semi-transparent materials (green/red).
* Updates material based on validity state.
* Disables colliders and scripts on the preview to prevent interference.

**Design Decisions:**
* Attached dynamically to the preview object, avoiding prefab modification.
* Creates materials at runtime to avoid asset dependencies.
* Uses material color (not shader) for simplicity and performance.

### RotationHandler

**Responsibility:** Provide utility functions for rotation calculations.

**Key Features:**
* Supports 0°, 90°, 180°, 270° rotations.
* Calculates size adjustments for rotated items.
* Provides offset calculations for center-based rotation.

**Design Decisions:**
* Implemented as a static utility class for stateless operations.
* Separates rotation logic from the controller for reusability.
* Supports future extensions (e.g., diagonal rotations, custom angles).

### PlacementUIManager

**Responsibility:** Manage UI elements and item selection.

**Key Features:**
* Dynamically creates item selection buttons.
* Displays rotation angle and placement status.
* Highlights selected item button.

**Design Decisions:**
* Decoupled from placement logic; communicates via `PlacementController`.
* Optional component; system works without UI.
* Uses TextMeshPro for modern text rendering.

## Data Flow

### Placement Workflow

1. **Item Selection**: User clicks a UI button or calls `SelectItem()`.
2. **Preview Creation**: `PlacementController` instantiates a preview from the item's prefab.
3. **Mouse Tracking**: Each frame, the controller raycasts to find the mouse position on the ground.
4. **Grid Snapping**: The hit point is converted to grid coordinates.
5. **Validity Check**: `GridManager` checks if the area is available.
6. **Preview Update**: The preview position and material are updated.
7. **Rotation**: User presses R to rotate; size is adjusted accordingly.
8. **Placement**: User clicks to place; `GridManager` marks the area as occupied and registers the item.
9. **Event Firing**: `OnItemPlaced` event is invoked for external systems to react.

### Removal Workflow

1. **Removal Request**: External code calls `RemoveItem(gridPosition)`.
2. **Item Lookup**: `GridManager` retrieves the placed item.
3. **Occupancy Update**: The area is marked as unoccupied.
4. **Unregistration**: The item is removed from the tracking dictionary.
5. **Destruction**: The GameObject is destroyed.
6. **Event Firing**: `OnItemRemoved` event is invoked.

## Coordinate Systems

### Grid Coordinates
* Origin at (0, 0) in the top-left corner.
* X increases to the right, Y increases downward.
* Used for occupancy checks and placement validation.

### World Coordinates
* Origin at `GridManager.GridOrigin`.
* Y-axis is up (standard Unity convention).
* Used for raycasting and GameObject positioning.

### Conversion
* **World to Grid**: Subtract origin, divide by cell size, round to nearest integer.
* **Grid to World**: Multiply by cell size, add origin, add half cell size for center.

## Extensibility

The system is designed to be extended without modifying core code:

### Adding Custom Placement Logic
```csharp
public class CustomPlacementLogic : MonoBehaviour
{
    private PlacementController controller;
    
    private void Start()
    {
        controller = FindObjectOfType<PlacementController>();
        controller.OnItemPlaced += HandleCustomLogic;
    }
    
    private void HandleCustomLogic(PlacedItem item)
    {
        // Custom logic here
    }
}
```

### Adding Custom Validation
Extend `PlacementController` or create a wrapper that adds additional validation checks before placement.

### Adding Custom Preview Visualization
Replace `PlacementPreview` with a custom component that provides different visual feedback (e.g., particle effects, sound).

## Performance Considerations

* **Occupancy Checks**: O(n) where n is the area size. Acceptable for typical grid sizes.
* **Raycasting**: Single raycast per frame. Optimized by limiting to ground layer.
* **Material Creation**: Done once during preview creation, not every frame.
* **Memory**: Grid occupancy is a 2D boolean array; for a 100x100 grid, this is ~10 KB.

## Future Enhancements

* **Undo/Redo System**: Store placement history and implement undo/redo functionality.
* **Saving/Loading**: Serialize placed items to JSON or binary format.
* **Multi-Layer Grids**: Support stacked grids for vertical placement.
* **Pathfinding Integration**: Calculate walkable paths around placed items.
* **Performance Optimization**: Use spatial hashing for large grids.
* **Advanced Rotation**: Support arbitrary rotation angles or diagonal placement.
