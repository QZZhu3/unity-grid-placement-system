using UnityEngine;

/// <summary>
/// Orchestrates the complete placement flow.
///
/// PlacementManager is a state coordinator only. It owns the active drag state
/// and the placement lifecycle, but delegates:
///   - Input polling and pick-up raycasting → PlacementInputHandler
///   - DraggableItem creation and setup     → PlacementFactory
///   - Grid validation                      → PlacementValidator (via DraggableItem)
///
/// Placement flow:
///   BeginPlacement(item)  → PlacementFactory.CreateFromItem()
///   PickUp(placed)        → PlacementFactory.AttachToDraggable()
///   HandleConfirm()       → updates GridManager, fires OnItemPlaced
///   HandleCancel()        → restores state, fires OnPlacementCancelled
///
/// PlacementManager is the only class that mutates GridManager occupancy.
/// </summary>
public class PlacementManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private GridManager      gridManager;
    [SerializeField] private PlacementFactory factory;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private DraggableItem activeDraggable;
    private PlacedItem    pickedUpItem;
    private Vector3       originalWorldPosition;
    private int           originalRotation;
    private bool          isHoveringReturnBasket;

    /// <summary>
    /// Frame counter set after a placement is confirmed.
    /// Prevents the same left-click that placed an item from immediately picking it back up.
    /// </summary>
    private int placementCooldownFrames;
    private const int PlacementCooldown = 2;

    // ── Events ────────────────────────────────────────────────────────────────

    public delegate void PlacementDelegate(PlacedItem item);
    public delegate void PickUpDelegate(PlacedItem item);
    public delegate void DragDelegate(DraggableItem item);

    /// <summary>Fired after an item is successfully placed or moved.</summary>
    public event PlacementDelegate OnItemPlaced;

    /// <summary>Fired after an item is picked up for moving.</summary>
    public event PickUpDelegate OnItemPickedUp;

    /// <summary>Fired when a drag is cancelled and the item returns to its origin.</summary>
    public event PlacementDelegate OnPlacementCancelled;

    /// <summary>Fired when an item starts being dragged (either new or picked up).</summary>
    public event DragDelegate OnDragStarted;

    /// <summary>Fired when an item stops being dragged (placed, cancelled, or returned).</summary>
    public event DragDelegate OnDragEnded;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (gridManager == null) gridManager = FindAnyObjectByType<GridManager>();
        if (factory     == null) factory     = FindAnyObjectByType<PlacementFactory>();
    }

    private void Update()
    {
        // Count down the post-placement cooldown.
        // PlacementInputHandler handles pick-up polling — nothing else needed here.
        if (placementCooldownFrames > 0)
            placementCooldownFrames--;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Begins dragging a new item from inventory.
    /// </summary>
    public void BeginPlacement(PlaceableItem itemData)
    {
        if (activeDraggable != null) return;

        DraggableItem draggable = factory.CreateFromItem(itemData);
        if (draggable == null) return;

        BindDraggable(draggable);
    }

    /// <summary>
    /// Picks up an already-placed item so the player can move it.
    /// Temporarily frees its grid cells.
    /// </summary>
    public void PickUp(PlacedItem item)
    {
        if (activeDraggable != null) return;
        if (placementCooldownFrames > 0) return;

        pickedUpItem          = item;
        originalWorldPosition = item.GameObject.transform.position;
        originalRotation      = item.Rotation;

        // Free the cells so the item doesn't block itself during validation
        gridManager.MarkAreaUnoccupied(item.GridPosition, item.Size);
        gridManager.UnregisterPlacedItem(item.GridPosition);

        DraggableItem draggable = factory.AttachToDraggable(item);
        if (draggable == null) return;

        BindDraggable(draggable);
        OnItemPickedUp?.Invoke(item);
    }

    /// <summary>
    /// Sets whether the cursor is hovering over the return basket zone.
    /// Called by ReturnBasketZone.
    /// </summary>
    public void SetHoveringReturnBasket(bool isHovering)
    {
        isHoveringReturnBasket = isHovering;
    }

    /// <summary>
    /// Programmatically cancels any active drag.
    /// Safe to call when no drag is active.
    /// </summary>
    public void CancelPlacement()
    {
        if (activeDraggable == null) return;
        HandleCancel(activeDraggable);
    }

    /// <summary>
    /// Programmatically returns the active draggable to inventory.
    /// Equivalent to the player dropping the item on the return basket.
    /// Safe to call when no drag is active.
    /// </summary>
    public void ReturnToInventory()
    {
        if (activeDraggable == null) return;
        ReturnToInventory(activeDraggable);
    }

    // ── State queries ─────────────────────────────────────────────────────────

    public bool IsDragging => activeDraggable != null;

    // ── Private ───────────────────────────────────────────────────────────────

    private void BindDraggable(DraggableItem draggable)
    {
        draggable.OnConfirm += HandleConfirm;
        draggable.OnCancel  += HandleCancel;
        activeDraggable      = draggable;
        isHoveringReturnBasket = false;

        OnDragStarted?.Invoke(activeDraggable);
    }

    private void HandleConfirm(DraggableItem draggable, Vector2Int gridPos, int rotation)
    {
        if (isHoveringReturnBasket)
        {
            ReturnToInventory(draggable);
            return;
        }

        Vector2Int footprint = draggable.GetFootprint();

        gridManager.MarkAreaOccupied(gridPos, footprint);

        draggable.transform.position = gridManager.GridToWorld(gridPos);
        draggable.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        factory.RestoreColliders(draggable.gameObject);

        // Build or update PlacedItem record
        PlacedItem placed;
        if (pickedUpItem != null)
        {
            pickedUpItem.GridPosition = gridPos;
            pickedUpItem.Size         = footprint;
            pickedUpItem.Rotation     = rotation;
            pickedUpItem.RefreshOccupiedCells();
            placed = pickedUpItem;
        }
        else
        {
            placed = new PlacedItem(
                draggable.ItemData.ItemId,
                gridPos,
                footprint,
                rotation,
                draggable.gameObject
            );
        }

        gridManager.RegisterPlacedItem(gridPos, placed);

        // Restore materials and remove preview component
        PlacementPreview preview = draggable.GetComponent<PlacementPreview>();
        if (preview != null)
        {
            preview.RestoreOriginalMaterials();
            Destroy(preview);
        }

        CleanupDraggable(draggable);
        placementCooldownFrames = PlacementCooldown;
        OnItemPlaced?.Invoke(placed);
    }

    private void HandleCancel(DraggableItem draggable)
    {
        if (pickedUpItem != null)
        {
            draggable.transform.position = originalWorldPosition;
            draggable.transform.rotation = Quaternion.Euler(0f, originalRotation, 0f);

            gridManager.MarkAreaOccupied(pickedUpItem.GridPosition, pickedUpItem.Size);
            gridManager.RegisterPlacedItem(pickedUpItem.GridPosition, pickedUpItem);

            factory.RestoreColliders(draggable.gameObject);
            OnPlacementCancelled?.Invoke(pickedUpItem);
        }
        else
        {
            Destroy(draggable.gameObject);
        }

        CleanupDraggable(draggable);
    }

    private void ReturnToInventory(DraggableItem draggable)
    {
        // For existing items: cells were already freed during PickUp.
        // Destroying the GameObject leaves inventory at +1 (correct).
        // For new items: inventory hasn't been deducted yet; destroying is net 0 (correct).
        Destroy(draggable.gameObject);
        CleanupDraggable(draggable);
    }

    private void CleanupDraggable(DraggableItem draggable)
    {
        draggable.OnConfirm -= HandleConfirm;
        draggable.OnCancel  -= HandleCancel;

        DraggableItem cached = draggable;
        DestroyImmediate(draggable);
        activeDraggable        = null;
        pickedUpItem           = null;
        isHoveringReturnBasket = false;

        OnDragEnded?.Invoke(cached);
    }
}
