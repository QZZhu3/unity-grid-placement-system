// ──────────────────────────────────────────────────────────────────────────────
// PlacementUIManager.cs  —  DEPRECATED
//
// This was the UI driver for the old PlacementController architecture.
// It has been replaced by InventoryUI + InventoryPlacementBridge.
//
// To fully remove this component:
//   1. Remove PlacementUIManager from any GameObjects in the scene.
//   2. Remove PlacementController from any GameObjects in the scene.
//   3. Delete both files.
// ──────────────────────────────────────────────────────────────────────────────

using UnityEngine;

/// <summary>
/// Deprecated legacy UI manager. Immediately disables itself at runtime to
/// prevent conflicts with the new InventoryUI-driven placement system.
/// Remove this component from the scene and delete this file.
/// </summary>
[System.Obsolete("PlacementUIManager is deprecated. Use InventoryUI instead.")]
public class PlacementUIManager : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning(
            "[PlacementUIManager] This component is deprecated. " +
            "Remove it from the scene and use InventoryUI instead.");
        enabled = false;
    }
}
