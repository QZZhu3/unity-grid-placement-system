# Grid-Based Placement System - API Reference

This document provides detailed API documentation for all components in the placement system.

## GridManager

The `GridManager` class handles all grid-related operations including world-to-grid conversion, occupancy tracking, and area validation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `GridWidth` | `int` | Read-only. The width of the grid in cells. |
| `GridHeight` | `int` | Read-only. The height of the grid in cells. |
| `CellSize` | `float` | Read-only. The size of each grid cell in world units. |
| `GridOrigin` | `Vector3` | Read-only. The world position of the grid's origin (0,0). |

### Methods

#### `Vector2Int WorldToGrid(Vector3 worldPosition)`
Converts a world position to grid coordinates. Uses rounding to find the nearest grid cell.

**Parameters:**
* `worldPosition` - The world position to convert.

**Returns:** Grid coordinates as `Vector2Int`.

#### `Vector3 GridToWorld(Vector2Int gridPosition)`
Converts grid coordinates to world position. Returns the center of the grid cell.

**Parameters:**
* `gridPosition` - The grid coordinates to convert.

**Returns:** World position as `Vector3`.

#### `bool IsWithinBounds(Vector2Int gridPosition)`
Checks if a single grid cell is within the grid bounds.

**Parameters:**
* `gridPosition` - The grid position to check.

**Returns:** `true` if within bounds, `false` otherwise.

#### `bool IsAreaWithinBounds(Vector2Int gridPosition, Vector2Int size)`
Checks if a rectangular area is completely within grid bounds.

**Parameters:**
* `gridPosition` - The top-left corner of the area.
* `size` - The width and height of the area in cells.

**Returns:** `true` if the entire area is within bounds, `false` otherwise.

#### `bool IsCellOccupied(Vector2Int gridPosition)`
Checks if a specific cell is occupied.

**Parameters:**
* `gridPosition` - The grid position to check.

**Returns:** `true` if occupied or out of bounds, `false` if available.

#### `bool IsAreaAvailable(Vector2Int gridPosition, Vector2Int size)`
Checks if an entire rectangular area is available for placement.

**Parameters:**
* `gridPosition` - The top-left corner of the area.
* `size` - The width and height of the area in cells.

**Returns:** `true` if all cells are available and within bounds, `false` otherwise.

#### `void MarkAreaOccupied(Vector2Int gridPosition, Vector2Int size)`
Marks a rectangular area as occupied on the grid.

**Parameters:**
* `gridPosition` - The top-left corner of the area.
* `size` - The width and height of the area in cells.

#### `void MarkAreaUnoccupied(Vector2Int gridPosition, Vector2Int size)`
Marks a rectangular area as unoccupied on the grid.

**Parameters:**
* `gridPosition` - The top-left corner of the area.
* `size` - The width and height of the area in cells.

#### `void RegisterPlacedItem(Vector2Int gridPosition, PlacedItem item)`
Registers a placed item in the tracking dictionary.

**Parameters:**
* `gridPosition` - The grid position of the item.
* `item` - The `PlacedItem` object to register.

#### `void UnregisterPlacedItem(Vector2Int gridPosition)`
Unregisters a placed item from the tracking dictionary.

**Parameters:**
* `gridPosition` - The grid position of the item to unregister.

#### `PlacedItem GetPlacedItem(Vector2Int gridPosition)`
Retrieves a placed item at a specific grid position.

**Parameters:**
* `gridPosition` - The grid position to query.

**Returns:** The `PlacedItem` at that position, or `null` if none exists.

#### `Dictionary<Vector2Int, PlacedItem> GetAllPlacedItems()`
Retrieves all placed items on the grid.

**Returns:** A dictionary mapping grid positions to `PlacedItem` objects.

#### `void ClearGrid()`
Clears all placements and resets the grid to an empty state.

---

## PlacementController

The `PlacementController` class manages the placement interaction, including item selection, preview display, grid snapping, and placement confirmation.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CanPlace` | `bool` | Read-only. Whether the current preview position is valid for placement. |

### Methods

#### `void SelectItem(PlaceableItem item)`
Selects an item to place and creates a preview object.

**Parameters:**
* `item` - The `PlaceableItem` to select, or `null` to deselect.

#### `void DeselectItem()`
Deselects the current item and destroys the preview.

#### `void RemoveItem(Vector2Int gridPosition)`
Removes a placed item from the grid.

**Parameters:**
* `gridPosition` - The grid position of the item to remove.

#### `PlaceableItem GetSelectedItem()`
Gets the currently selected item.

**Returns:** The selected `PlaceableItem`, or `null` if none is selected.

#### `int GetCurrentRotation()`
Gets the current rotation angle of the preview.

**Returns:** The rotation angle (0, 90, 180, or 270).

### Events

#### `event ItemPlacedDelegate OnItemPlaced`
Invoked when an item is successfully placed on the grid.

**Delegate Signature:**
```csharp
public delegate void ItemPlacedDelegate(PlacedItem item);
```

#### `event ItemRemovedDelegate OnItemRemoved`
Invoked when an item is removed from the grid.

**Delegate Signature:**
```csharp
public delegate void ItemRemovedDelegate(PlacedItem item);
```

---

## PlaceableItem

The `PlaceableItem` ScriptableObject defines the properties of a placeable item.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | `string` | Unique identifier for the item. |
| `Prefab` | `GameObject` | The prefab to instantiate when placing the item. |
| `Size` | `Vector2Int` | The grid footprint size (width, height). |
| `DisplayName` | `string` | Human-readable name for UI display. |
| `Description` | `string` | Item description for UI tooltips. |
| `Icon` | `Sprite` | UI icon for item selection buttons. |

---

## PlacedItem

The `PlacedItem` class represents an item that has been placed on the grid.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ItemId` | `string` | The ID of the item that was placed. |
| `GridPosition` | `Vector2Int` | The grid coordinates where the item is placed. |
| `Size` | `Vector2Int` | The actual footprint size (accounting for rotation). |
| `Rotation` | `int` | The rotation angle (0, 90, 180, or 270). |
| `GameObject` | `GameObject` | The instantiated GameObject in the scene. |

---

## RotationHandler

The `RotationHandler` utility class provides static methods for rotation calculations.

### Static Methods

#### `int GetNextRotation(int currentRotation)`
Gets the next rotation angle in the cycle (0° → 90° → 180° → 270° → 0°).

**Parameters:**
* `currentRotation` - The current rotation angle.

**Returns:** The next rotation angle.

#### `Quaternion AngleToQuaternion(int rotationAngle)`
Converts a rotation angle to a quaternion.

**Parameters:**
* `rotationAngle` - The rotation angle (0, 90, 180, or 270).

**Returns:** The corresponding `Quaternion`.

#### `int QuaternionToAngle(Quaternion rotation)`
Converts a quaternion to the nearest valid rotation angle.

**Parameters:**
* `rotation` - The quaternion to convert.

**Returns:** The nearest valid rotation angle (0, 90, 180, or 270).

#### `Vector2Int GetAdjustedSize(Vector2Int originalSize, int rotationAngle)`
Calculates the adjusted footprint size based on rotation. For 90° and 270° rotations, width and height are swapped.

**Parameters:**
* `originalSize` - The original size.
* `rotationAngle` - The rotation angle.

**Returns:** The adjusted size.

#### `Vector2 RotatePoint(Vector2 point, int rotationAngle)`
Rotates a 2D point around the origin by the specified angle.

**Parameters:**
* `point` - The point to rotate.
* `rotationAngle` - The rotation angle in degrees.

**Returns:** The rotated point.

#### `Vector2Int GetRotationOffsetAdjustment(Vector2Int originalSize, int fromRotation, int toRotation)`
Calculates the grid offset adjustment when rotating an item to ensure it rotates around its center.

**Parameters:**
* `originalSize` - The original item size.
* `fromRotation` - The current rotation angle.
* `toRotation` - The target rotation angle.

**Returns:** The offset adjustment as `Vector2Int`.

#### `bool IsValidRotation(int rotationAngle)`
Validates if a rotation angle is valid.

**Parameters:**
* `rotationAngle` - The rotation angle to validate.

**Returns:** `true` if the angle is valid (0, 90, 180, or 270), `false` otherwise.

---

## PlacementPreview

The `PlacementPreview` component manages the visual feedback of the placement preview.

### Methods

#### `void SetValidity(bool valid)`
Sets the validity state of the preview. Updates material color (green for valid, red for invalid).

**Parameters:**
* `valid` - `true` for valid placement, `false` for invalid.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsValid` | `bool` | Read-only. The current validity state. |

---

## PlacementUIManager

The `PlacementUIManager` class manages the UI for item selection and status display.

### Methods

#### `void DeselectItem()`
Deselects the current item and resets button highlighting.

---

## Usage Examples

### Example 1: Basic Setup
```csharp
// In your initialization script
GridManager gridManager = GetComponent<GridManager>();
PlacementController controller = GetComponent<PlacementController>();

// Select an item to place
PlaceableItem wallItem = Resources.Load<PlaceableItem>("Items/WallItem");
controller.SelectItem(wallItem);
```

### Example 2: Listening to Placement Events
```csharp
PlacementController controller = FindObjectOfType<PlacementController>();

controller.OnItemPlaced += (item) =>
{
    Debug.Log($"Placed {item.ItemId} at {item.GridPosition}");
};

controller.OnItemRemoved += (item) =>
{
    Debug.Log($"Removed {item.ItemId}");
};
```

### Example 3: Removing Items
```csharp
PlacementController controller = FindObjectOfType<PlacementController>();
GridManager gridManager = FindObjectOfType<GridManager>();

// Remove item at grid position (5, 5)
controller.RemoveItem(new Vector2Int(5, 5));
```

### Example 4: Querying Grid State
```csharp
GridManager gridManager = FindObjectOfType<GridManager>();

// Check if an area is available
Vector2Int position = new Vector2Int(3, 3);
Vector2Int size = new Vector2Int(2, 2);

if (gridManager.IsAreaAvailable(position, size))
{
    Debug.Log("Area is available for placement");
}
```
