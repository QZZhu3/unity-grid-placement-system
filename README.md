# Unity Grid-Based Placement System

A modular, production-ready grid-based placement system for Unity. Place items on a grid with real-time preview, rotation support, and occupancy validation.

## Features

* **Grid Management**: Efficient 2D grid with world-to-grid coordinate conversion.
* **Item Placement**: Place items with real-time preview and validity feedback.
* **Rotation Support**: Rotate items in 90° increments with automatic footprint adjustment.
* **Occupancy Tracking**: Prevent overlapping placements with occupancy validation.
* **Raycasting Integration**: Snap items to grid based on mouse position over ground.
* **Visual Feedback**: Green preview for valid placement, red for invalid.
* **Event System**: Subscribe to placement events for custom logic.
* **Modular Architecture**: Loosely coupled components for easy extension.
* **ScriptableObject Integration**: Define items in the editor without code.

## Project Structure

```
UnityPlacementSystem/
├── Scripts/
│   ├── GridManager.cs              # Core grid system
│   ├── PlaceableItem.cs            # Item definition (ScriptableObject)
│   ├── PlacementController.cs      # Main placement logic
│   ├── PlacementPreview.cs         # Visual feedback
│   ├── PlacementUIManager.cs       # UI integration
│   └── RotationHandler.cs          # Rotation utilities
├── Documentation/
│   ├── QuickStart.md               # 5-minute setup guide
│   ├── SceneSetupGuide.md          # Detailed scene configuration
│   ├── APIReference.md             # Complete API documentation
│   └── Architecture.md             # System design and extensibility
└── README.md                        # This file
```

## Quick Start

### 1. Import Scripts
Copy all scripts from the `Scripts` folder into your Unity project.

### 2. Set Up Scene
* Create a ground plane with a collider on the `Ground` layer.
* Add `GridManager` and `PlacementController` to an empty GameObject.
* Configure grid dimensions and cell size.

### 3. Create Items
* Right-click in Project: `Create > Placement System > Placeable Item`
* Configure item properties (prefab, size, ID).

### 4. Test
* Press Play and move your mouse over the ground.
* Press R to rotate, click to place.

For detailed instructions, see **QuickStart.md**.

## Core Components

| Component | Purpose |
|-----------|---------|
| **GridManager** | Manages grid data, occupancy, and coordinate conversion. |
| **PlacementController** | Handles user input, preview, and placement logic. |
| **PlaceableItem** | ScriptableObject defining item properties. |
| **PlacementPreview** | Provides visual feedback (green/red materials). |
| **RotationHandler** | Utility functions for rotation calculations. |
| **PlacementUIManager** | Optional UI for item selection and status display. |

## Usage Example

```csharp
// Select an item to place
PlaceableItem wallItem = Resources.Load<PlaceableItem>("Items/Wall");
PlacementController controller = FindObjectOfType<PlacementController>();
controller.SelectItem(wallItem);

// Listen to placement events
controller.OnItemPlaced += (item) =>
{
    Debug.Log($"Placed {item.ItemId} at {item.GridPosition}");
};

// Remove an item
controller.RemoveItem(new Vector2Int(5, 5));
```

## Controls

* **Mouse Move**: Preview follows mouse, snapped to grid.
* **R Key**: Rotate preview by 90°.
* **Left Click**: Place item at preview location.

## Architecture

The system follows these design principles:

* **Separation of Concerns**: Each component has a single responsibility.
* **Loose Coupling**: Components communicate via events and public methods.
* **Extensibility**: Add custom logic by subscribing to events or extending components.
* **Performance**: Optimized for typical grid sizes (up to 100x100).

See **Architecture.md** for detailed design documentation.

## API Reference

All public methods and properties are documented in **APIReference.md**. Key methods include:

* `GridManager.WorldToGrid()` - Convert world position to grid coordinates.
* `GridManager.IsAreaAvailable()` - Check if an area is available for placement.
* `PlacementController.SelectItem()` - Select an item to place.
* `PlacementController.RemoveItem()` - Remove a placed item.

## Extending the System

### Custom Placement Logic

```csharp
public class PlacementAudioFeedback : MonoBehaviour
{
    private void Start()
    {
        PlacementController controller = FindObjectOfType<PlacementController>();
        controller.OnItemPlaced += PlaySound;
    }
    
    private void PlaySound(PlacedItem item)
    {
        AudioSource.PlayClipAtPoint(placementSound, item.GameObject.transform.position);
    }
}
```

### Custom Validation

Extend `PlacementController` to add additional validation checks:

```csharp
public class CustomPlacementController : PlacementController
{
    protected override bool ValidatePlacement(Vector2Int position, Vector2Int size)
    {
        // Add custom validation logic
        return base.ValidatePlacement(position, size) && CustomCheck(position);
    }
}
```

See **Architecture.md** for more extension examples.

## Performance

* **Grid Lookup**: O(1) for single cell checks.
* **Area Validation**: O(n) where n is the area size.
* **Raycasting**: Single raycast per frame (optimized to ground layer).
* **Memory**: ~10 KB for a 100x100 grid.

## Troubleshooting

**Preview not showing?**
* Ensure ground plane has a collider and is on the `Ground` layer.
* Verify `Ground Layer Mask` in `PlacementController` includes the `Ground` layer.

**Items placing off-grid?**
* Check prefab pivot point alignment.
* Verify `Grid Origin` matches ground plane position.

**Rotation not working?**
* Ensure `PlacementController` is active and receiving input.
* Check that R key isn't consumed by another system.

For more troubleshooting, see **SceneSetupGuide.md**.

## Requirements

* Unity 2020.3 LTS or later
* TextMeshPro (included in recent Unity versions)

## License

This system is provided as-is for use in your projects.

## Documentation

* **QuickStart.md** - Get started in 5 minutes.
* **SceneSetupGuide.md** - Detailed scene configuration and setup.
* **APIReference.md** - Complete API documentation with examples.
* **Architecture.md** - System design, data flow, and extensibility guide.

## Support

For issues or questions, refer to the documentation files or review the inline code comments for detailed explanations of each component's functionality.
