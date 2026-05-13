using UnityEngine;

/// <summary>
/// Handles pick-up input polling for the placement system.
///
/// Responsibilities:
///   - Each frame, checks whether the player has pressed the pick-up button
///   - Raycasts from the cursor to find placed objects
///   - Notifies PlacementManager when an item should be picked up
///
/// This class owns no placement state. It only reads input and raycasts.
/// PlacementManager owns the drag state and calls PickUp() when notified.
///
/// Attach to: PlacementSystem (alongside PlacementManager and MouseInputController).
/// </summary>
public class PlacementInputHandler : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private PlacementManager      placementManager;
    [SerializeField] private GridManager           gridManager;
    [SerializeField] private MouseInputController  mouseInput;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (placementManager == null) placementManager = FindAnyObjectByType<PlacementManager>();
        if (gridManager      == null) gridManager      = FindAnyObjectByType<GridManager>();
        if (mouseInput       == null) mouseInput       = FindAnyObjectByType<MouseInputController>();
    }

    private void Update()
    {
        // Only poll for pick-up when PlacementManager is idle
        if (placementManager == null || placementManager.IsDragging) return;

        IInputController input = mouseInput;
        if (input == null || !input.PickUpPressed) return;

        TryPickUpAtCursor();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void TryPickUpAtCursor()
    {
        if (mouseInput == null || gridManager == null || placementManager == null) return;

        Ray ray = mouseInput.MainCamera.ScreenPointToRay(mouseInput.ScreenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, mouseInput.RaycastDistance)) return;

        GameObject hitRoot = hit.collider.transform.root.gameObject;

        foreach (var kvp in gridManager.GetAllPlacedItems())
        {
            if (kvp.Value.GameObject == hitRoot)
            {
                placementManager.PickUp(kvp.Value);
                return;
            }
        }
    }
}
