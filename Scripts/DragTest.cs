// ------------------------------------------------------------------------------
// DragTest.cs  --  DEPRECATED
//
// This file is kept as a stub so Unity does not throw missing-script errors on
// any GameObjects that still reference it in the scene.
//
// The auto-spawn prototype workflow has been replaced by the inventory-driven
// placement flow. Players now select items by clicking/tapping slots in the
// Inventory UI panel, which calls PlacementManager.BeginPlacement() directly.
//
// To fully remove this component:
//   1. Remove the DragTest component from the PlacementSystem GameObject in the scene.
//   2. Delete this file.
//   3. The InventoryUI component on the InventoryPanel GameObject drives placement.
// ------------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Deprecated prototype test driver. No longer performs any auto-spawning.
/// Remove this component from the scene and delete this file once the
/// Inventory UI is fully wired up.
/// </summary>
[System.Obsolete("DragTest is deprecated. Use InventoryUI to drive placement.")]
public class DragTest : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning(
            "[DragTest] This component is deprecated and does nothing. " +
            "Remove it from the scene and wire up InventoryUI instead.");
    }
}
