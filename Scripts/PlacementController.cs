// ------------------------------------------------------------------------------
// PlacementController.cs  --  DEPRECATED
//
// This was the original monolithic placement controller.
// It has been replaced by the PlacementManager + DraggableItem + MouseInputController
// + PlacementValidator architecture.
//
// To fully remove this component:
//   1. Remove PlacementController from any GameObjects in the scene.
//   2. Remove PlacementUIManager from any GameObjects in the scene.
//   3. Delete both files.
// ------------------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Deprecated legacy placement controller. Immediately disables itself at runtime
/// to prevent conflicts with the new PlacementManager-driven system.
/// Remove this component from the scene and delete this file.
/// </summary>
[System.Obsolete("PlacementController is deprecated. Use PlacementManager instead.")]
public class PlacementController : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning(
            "[PlacementController] This component is deprecated. " +
            "Remove it from the scene and use PlacementManager instead.");
        enabled = false;
    }

    // -- Stub public API -------------------------------------------------------
    // These stubs prevent compile errors if any other script still references
    // PlacementController methods. Remove them once all references are cleared.

    public void SelectItem(PlaceableItem item) { }
    public void DeselectItem() { }
    public int  GetCurrentRotation() => 0;
    public PlaceableItem GetSelectedItem() => null;
    public bool CanPlace => false;
}
