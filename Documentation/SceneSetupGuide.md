# Unity Grid-Based Placement System Guide

This document outlines how to set up and use the modular grid-based placement system in your Unity project. The system includes grid management, item placement with preview, rotation support, and UI integration.

## Architecture Overview

The system is built on four core components:

| Component | Description |
|-----------|-------------|
| **GridManager** | Handles the underlying grid data structure, world-to-grid conversion, and tracks cell occupancy. |
| **PlaceableItem** | A ScriptableObject that defines the properties of an item (prefab, size, ID, UI icon). |
| **PlacementController** | Manages the placement interaction, raycasting, preview visualization, rotation, and final placement. |
| **PlacementPreview** | Handles the visual feedback (green/red materials) for valid/invalid placement locations. |

## 1. Scene Setup Instructions

To get the system working in your scene, follow these steps:

### Ground Plane Setup
1. Create a 3D Plane or Cube to serve as your ground (`GameObject > 3D Object > Plane`).
2. Ensure it has a **Collider** attached (BoxCollider or MeshCollider).
3. Create a new Layer named `Ground` (`Edit > Project Settings > Tags and Layers`).
4. Assign the `Ground` layer to your ground object.

### Manager Setup
1. Create an Empty GameObject named `PlacementSystem`.
2. Attach the `GridManager` script to it.
   * Configure the `Grid Width`, `Grid Height`, and `Cell Size` in the inspector.
3. Attach the `PlacementController` script to the same object.
   * Assign the `GridManager` reference (drag and drop the component).
   * Assign the Main Camera to the `Main Camera` field.
   * Set the `Ground Layer Mask` to only include your `Ground` layer.

### UI Setup (Optional but Recommended)
1. Create a Canvas (`GameObject > UI > Canvas`).
2. Create an Empty GameObject named `PlacementUI` under the Canvas.
3. Attach the `PlacementUIManager` script.
4. Assign the `PlacementController` reference.
5. Create a UI Text (TextMeshPro) for rotation display and status display, and assign them.
6. Create a Horizontal Layout Group for item buttons, and assign it to `Item Button Container`.
7. Create a button prefab and assign it to `Item Button Prefab`.

## 2. Creating Placeable Items

To create items that can be placed on the grid:

1. In your Project window, right-click and select `Create > Placement System > Placeable Item`.
2. Name the new ScriptableObject asset (e.g., "WallItem").
3. In the Inspector, configure the item:
   * **Item Id**: Unique identifier (e.g., "wall_01").
   * **Prefab**: The 3D model/prefab to instantiate.
   * **Size**: The footprint on the grid (e.g., X: 1, Y: 3 for a wall).
   * **Display Name**: Name shown in UI.

### Prefab Requirements
* The prefab's pivot point should ideally be at the bottom center or bottom corner, depending on your grid visual alignment.
* The system automatically handles disabling colliders during the preview phase.

## 3. Controls and Usage

Once the scene is set up and items are configured:

* **Select Item**: Call `PlacementController.SelectItem(item)` (usually via UI buttons).
* **Move Mouse**: The preview object will snap to the grid based on the mouse position over the ground layer.
* **Rotate**: Press **R** to rotate the item by 90 degrees. The footprint size automatically adjusts (e.g., a 1x3 item becomes 3x1).
* **Place**: Left-click to place the item. The grid cells will be marked as occupied, preventing overlapping placements.

## 4. Extending the System

The system is designed to be modular. You can extend it by subscribing to the events in `PlacementController`:

```csharp
// Example extension script
public class PlacementAudioFeedback : MonoBehaviour
{
    private PlacementController controller;
    
    private void Start()
    {
        controller = FindObjectOfType<PlacementController>();
        controller.OnItemPlaced += HandleItemPlaced;
    }
    
    private void HandleItemPlaced(PlacedItem item)
    {
        // Play placement sound
        Debug.Log($"Placed {item.ItemId} at {item.GridPosition}");
    }
}
```

## Troubleshooting

* **Preview not showing or snapping**: Ensure your ground object has a collider and is set to the correct layer matching the `Ground Layer Mask` in the PlacementController.
* **Items placing slightly off-grid**: Check the pivot point of your prefabs. The `GridManager.GridToWorld` method calculates the center of the cell. If your prefab's pivot is at a corner, you may need to adjust the instantiation position logic.
* **Red preview everywhere**: Ensure the `GridManager` dimensions are large enough and the grid origin aligns with your ground plane. Out-of-bounds areas are considered invalid.
