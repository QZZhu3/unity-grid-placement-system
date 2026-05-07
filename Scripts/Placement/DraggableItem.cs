using UnityEngine;

/// <summary>
/// Attached to a GameObject while it is being dragged.
/// Follows the cursor via IInputController, snaps to the grid,
/// shows green/red validity feedback, and supports R-key rotation.
///
/// DraggableItem never reads input directly and never touches the grid.
/// It asks PlacementValidator whether the current position is valid,
/// and notifies PlacementManager when confirm/cancel is pressed.
/// </summary>
[RequireComponent(typeof(PlacementPreview))]
public class DraggableItem : MonoBehaviour
{
    // ── Configuration ─────────────────────────────────────────────────────────

    private GridManager       gridManager;
    private PlacementValidator validator;
    private IInputController   input;

    private PlaceableItem      itemData;
    private PlacementPreview   preview;

    /// <summary>
    /// When moving an existing item, store its original grid position so
    /// PlacementValidator can exclude its own footprint from occupancy checks.
    /// Null when placing a brand-new item from inventory.
    /// </summary>
    private Vector2Int? originalGridPosition;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private int        currentRotation  = 0;       // 0, 90, 180, 270
    private Vector2Int currentGridPos   = Vector2Int.zero;
    private bool       isValid          = false;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired when the player confirms placement (left-click).</summary>
    public event System.Action<DraggableItem, Vector2Int, int> OnConfirm;

    /// <summary>Fired when the player cancels placement (right-click / Escape).</summary>
    public event System.Action<DraggableItem> OnCancel;

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>
    /// Initialises the draggable state. Call this immediately after adding the component.
    /// </summary>
    public void Initialise(PlaceableItem data,
                           GridManager gm,
                           PlacementValidator pv,
                           IInputController ic,
                           Vector2Int? originalPos = null)
    {
        itemData             = data;
        gridManager          = gm;
        validator            = pv;
        input                = ic;
        originalGridPosition = originalPos;
        currentRotation      = 0;

        preview = GetComponent<PlacementPreview>();
    }

    // ── MonoBehaviour ─────────────────────────────────────────────────────────

    private void Update()
    {
        HandleRotation();
        UpdatePosition();
        HandleConfirmCancel();
    }

    // ── Private logic ─────────────────────────────────────────────────────────

    private void HandleRotation()
    {
        if (!input.RotatePressed) return;
        currentRotation = (currentRotation + 90) % 360;
        transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    private void UpdatePosition()
    {
        Vector3 worldPos = input.WorldPosition;
        if (worldPos == Vector3.zero) return;   // no valid ground hit this frame

        currentGridPos = gridManager.WorldToGrid(worldPos);
        Vector2Int footprint = GetFootprint();

        ValidationResult result = validator.Validate(
            currentGridPos, footprint, originalGridPosition);

        isValid = result.IsValid;
        preview.SetValidity(isValid);

        // Snap to grid centre
        transform.position = gridManager.GridToWorld(currentGridPos);
    }

    private void HandleConfirmCancel()
    {
        if (input.ConfirmPressed && isValid)
        {
            OnConfirm?.Invoke(this, currentGridPos, currentRotation);
        }
        else if (input.CancelPressed)
        {
            OnCancel?.Invoke(this);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the item's size footprint adjusted for the current rotation.
    /// Width and depth are swapped for 90° and 270° rotations.
    /// </summary>
    public Vector2Int GetFootprint()
    {
        Vector2Int size = itemData.Size;
        return (currentRotation == 90 || currentRotation == 270)
            ? new Vector2Int(size.y, size.x)
            : size;
    }

    // ── Public accessors ──────────────────────────────────────────────────────

    public PlaceableItem  ItemData         => itemData;
    public int            CurrentRotation  => currentRotation;
    public Vector2Int     CurrentGridPos   => currentGridPos;
    public bool           IsValid          => isValid;
    public Vector2Int?    OriginalGridPos  => originalGridPosition;
}
