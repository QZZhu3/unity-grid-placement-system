# Quick Start Guide

Get the grid-based placement system up and running in 5 minutes.

## Step 1: Import Scripts

1. Copy all scripts from the `Scripts` folder into your Unity project's `Assets` folder.
2. Ensure all scripts are in the same namespace or adjust imports as needed.

## Step 2: Create the Scene

1. Create a new scene or open an existing one.
2. Create a **Ground Plane**:
   * `GameObject > 3D Object > Plane`
   * Scale it to fit your needs (e.g., 20x20 units).
   * Add a **BoxCollider** component.
   * Create a new Layer called `Ground` and assign it to the plane.

3. Create a **PlacementSystem** GameObject:
   * `GameObject > Create Empty`
   * Name it `PlacementSystem`
   * Add the `GridManager` script as a component.
   * Add the `PlacementController` script as a component.

## Step 3: Configure GridManager

In the Inspector, set the following on the `GridManager` component:

* **Grid Width**: 10
* **Grid Height**: 10
* **Cell Size**: 1.0
* **Grid Origin**: (0, 0, 0) or adjust to match your ground plane

## Step 4: Configure PlacementController

In the Inspector, set the following on the `PlacementController` component:

* **Grid Manager**: Drag the `GridManager` component from the scene.
* **Main Camera**: Drag the Main Camera from the scene.
* **Ground Layer Mask**: Select only the `Ground` layer.

## Step 5: Create a Placeable Item

1. Right-click in your Project folder.
2. Select `Create > Placement System > Placeable Item`.
3. Name it `TestItem`.
4. In the Inspector, configure:
   * **Item Id**: `test_item`
   * **Prefab**: Create a simple cube (`GameObject > 3D Object > Cube`) and drag it into this field.
   * **Size**: X: 1, Y: 1
   * **Display Name**: Test Item

## Step 6: Set Up UI (Optional)

1. Create a Canvas: `GameObject > UI > Canvas`
2. Create a Button under the Canvas: `GameObject > UI > Button`
3. Rename the button to `SelectItemButton`
4. Add the following script to the button:

```csharp
using UnityEngine;

public class ItemSelectionButton : MonoBehaviour
{
    [SerializeField] private PlaceableItem item;
    private PlacementController controller;
    
    private void Start()
    {
        controller = FindObjectOfType<PlacementController>();
        GetComponent<Button>().onClick.AddListener(() => controller.SelectItem(item));
    }
}
```

5. Assign the `TestItem` ScriptableObject to the button's `item` field.

## Step 7: Test

1. Press Play in the editor.
2. Move your mouse over the ground plane.
3. You should see a preview cube following your mouse, snapped to the grid.
4. Press **R** to rotate the preview.
5. Click to place the item.
6. The preview should turn red if you try to place over an existing item.

## Common Issues

**Preview not showing?**
* Ensure the ground plane has a collider and is on the `Ground` layer.
* Check that the `Ground Layer Mask` in `PlacementController` includes the `Ground` layer.

**Items placing off-grid?**
* Check that the prefab's pivot point is at the bottom-center or bottom-left corner.
* Verify that `Grid Origin` matches the position of your ground plane.

**Rotation not working?**
* Ensure the `PlacementController` script is active and receiving input.
* Check that the `R` key isn't being consumed by another system.

## Next Steps

* Read the **Scene Setup Guide** for detailed configuration options.
* Read the **API Reference** to understand all available methods and properties.
* Read the **Architecture** document to understand how to extend the system.
* Create additional `PlaceableItem` ScriptableObjects for different items.
* Implement custom logic by subscribing to `OnItemPlaced` and `OnItemRemoved` events.
