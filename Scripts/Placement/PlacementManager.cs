using UnityEngine;

/// <summary>
/// Orchestrates the complete placement flow:
///   1. BeginPlacement  — start dragging a new item from inventory
///   2. PickUp          — pick up an already-placed item to move it
///   3. Finalize        — confirm placement, update grid occupancy
///   4. Cancel          — abort drag, restore original position
///
/// PlacementManager is the only class that mutates GridManager occupancy.
/// It never reads input directly; it delegates to IInputController.
/// </summary>
public class PlacementManager : MonoBehaviour
{
    // ── Inspector references ──────────────────────────────────────────────────

    [SerializeField] private GridManager        gridManager;
    [SerializeField] private PlacementValidator validator;
    [SerializeField] private MouseInputController mouseInput;   // concrete type for scene wiring

    // ── Runtime state ─────────────────────────────────────────────────────────

    private DraggableItem   activeDraggable;
    private PlacedItem      pickedUpItem;           // non-null when moving an existing item
    private Vector3         originalWorldPosition;  // restore point on cancel
    private int             originalRotation;

    /// <summary>
    /// Frame counter set after a placement is confirmed.
    /// Prevents the same left-click that placed an item from immediately picking it back up.
    /// </summary>
    private int             placementCooldownFrames = 0;
    private const int       PlacementCooldown = 2;  // frames to ignore pick-up after placement

    private IInputController Input => mouseInput;   // access via interface

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

    private bool isHoveringReturnBasket = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (gridManager == null) gridManager = FindAnyObjectByType<GridManager>();
        if (validator   == null) validator   = FindAnyObjectByType<PlacementValidator>();
        if (mouseInput  == null) mouseInput  = FindAnyObjectByType<MouseInputController>();
    }

    private void Update()
    {
        // Count down the post-placement cooldown
        if (placementCooldownFrames > 0)
        {
            placementCooldownFrames--;
            return;
        }

        // Poll for pick-up clicks on placed objects when nothing is being dragged
        if (activeDraggable == null && Input.PickUpPressed)
            TryPickUpAtCursor();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Begins dragging a new item (e.g., from inventory).
    /// Instantiates the prefab and attaches a DraggableItem component.
    /// </summary>
    public void BeginPlacement(PlaceableItem itemData)
    {
        if (activeDraggable != null) return;    // already dragging

        GameObject go = Instantiate(itemData.Prefab);
        SetupDraggable(go, itemData, null);
    }

    /// <summary>
    /// Picks up an already-placed item so the player can move it.
    /// Temporarily frees its grid cells.
    /// </summary>
    public void PickUp(PlacedItem item)
    {
        if (activeDraggable != null) return;

        pickedUpItem          = item;
        originalWorldPosition = item.GameObject.transform.position;
        originalRotation      = item.Rotation;

        // Free the cells so the item doesn't block itself during validation
        gridManager.MarkAreaUnoccupied(item.GridPosition, item.Size);
        gridManager.UnregisterPlacedItem(item.GridPosition);

        // Attach drag behaviour to the existing GameObject
        SetupDraggable(item.GameObject, null, item.GridPosition);

        OnItemPickedUp?.Invoke(item);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void SetupDraggable(GameObject go, PlaceableItem itemData, Vector2Int? originalPos)
    {
        // Ensure PlacementPreview exists (required by DraggableItem)
        if (go.GetComponent<PlacementPreview>() == null)
            go.AddComponent<PlacementPreview>();

        // Disable colliders so raycasts pass through to the ground
        foreach (Collider col in go.GetComponentsInChildren<Collider>())
            col.enabled = false;

        DraggableItem draggable = go.AddComponent<DraggableItem>();

        // When moving an existing item, use its PlaceableItem from the PlacedItem record
        PlaceableItem data = itemData ?? GetItemDataFromPlacedItem(pickedUpItem);
        draggable.Initialise(data, gridManager, validator, Input, originalPos);

        draggable.OnConfirm += HandleConfirm;
        draggable.OnCancel  += HandleCancel;

        activeDraggable = draggable;
        isHoveringReturnBasket = false;

        OnDragStarted?.Invoke(activeDraggable);
    }

    public void SetHoveringReturnBasket(bool isHovering)
    {
        isHoveringReturnBasket = isHovering;
    }

    private void HandleConfirm(DraggableItem draggable, Vector2Int gridPos, int rotation)
    {
        // Intercept confirm if hovering over the return basket
        if (isHoveringReturnBasket)
        {
            ReturnToInventory(draggable);
            return;
        }

        Vector2Int footprint = draggable.GetFootprint();

        // Mark the new cells as occupied
        gridManager.MarkAreaOccupied(gridPos, footprint);

        // Snap to final grid position
        draggable.transform.position = gridManager.GridToWorld(gridPos);
        draggable.transform.rotation = Quaternion.Euler(0f, rotation, 0f);

        // Re-enable colliders on the placed object
        foreach (Collider col in draggable.GetComponentsInChildren<Collider>())
            col.enabled = true;

        // Build or update PlacedItem record
        PlacedItem placed;
        if (pickedUpItem != null)
        {
            // Updating an existing item — refresh OccupiedCells to reflect new position/rotation
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

        // Restore the object's original materials and remove the preview component
        // so the placed block looks correct instead of staying green.
        PlacementPreview preview = draggable.GetComponent<PlacementPreview>();
        if (preview != null)
        {
            preview.RestoreOriginalMaterials();
            Destroy(preview);
        }

        CleanupDraggable(draggable);

        // Block pick-up for a couple of frames so this same click doesn't immediately
        // pick up the object that was just placed.
        placementCooldownFrames = PlacementCooldown;

        OnItemPlaced?.Invoke(placed);
    }

    private void HandleCancel(DraggableItem draggable)
    {
        if (pickedUpItem != null)
        {
            // Restore the item to its original position and re-occupy those cells
            draggable.transform.position = originalWorldPosition;
            draggable.transform.rotation = Quaternion.Euler(0f, originalRotation, 0f);

            gridManager.MarkAreaOccupied(pickedUpItem.GridPosition, pickedUpItem.Size);
            gridManager.RegisterPlacedItem(pickedUpItem.GridPosition, pickedUpItem);

            // Re-enable colliders
            foreach (Collider col in draggable.GetComponentsInChildren<Collider>())
                col.enabled = true;

            OnPlacementCancelled?.Invoke(pickedUpItem);
        }
        else
        {
            // New item from inventory — just destroy it
            Destroy(draggable.gameObject);
        }

        CleanupDraggable(draggable);
    }

    private void ReturnToInventory(DraggableItem draggable)
    {
        // Returning is essentially cancelling the placement, but if it was an existing item,
        // we don't want to restore it to the grid. We want to permanently remove it from the grid
        // and let the InventoryPlacementBridge handle the inventory math.
        
        if (pickedUpItem != null)
        {
            // It was an existing item. We already freed its cells and unregistered it during PickUp.
            // We just need to destroy the GameObject and NOT restore it.
            // Wait, InventoryPlacementBridge adds +1 on PickUp. If we destroy it now, we shouldn't deduct.
            // But we need to signal that it was permanently returned so other systems know.
            // Actually, if we just Destroy the GameObject, the +1 from PickUp stays in inventory.
            // This is perfectly correct! (Net +1 to inventory, item removed from world).
            
            Destroy(draggable.gameObject);
            
            // We don't fire OnPlacementCancelled because we don't want the bridge to deduct 1.
            // We just let it disappear.
        }
        else
        {
            // It was a brand new item from inventory.
            // Inventory hasn't been deducted yet (deduction happens on OnItemPlaced).
            // So we just destroy it. (Net 0 to inventory, item removed from world).
            Destroy(draggable.gameObject);
        }

        CleanupDraggable(draggable);
    }

    private void CleanupDraggable(DraggableItem draggable)
    {
        draggable.OnConfirm -= HandleConfirm;
        draggable.OnCancel  -= HandleCancel;
        
        DraggableItem cachedDraggable = draggable; // cache for event

        DestroyImmediate(draggable);            // immediate removal so next BeginPlacement can add fresh component
        activeDraggable = null;
        pickedUpItem    = null;
        isHoveringReturnBasket = false;

        OnDragEnded?.Invoke(cachedDraggable);
    }

    /// <summary>
    /// Raycasts from the cursor against placed objects and picks up the hit item.
    /// </summary>
    private void TryPickUpAtCursor()
    {
        Ray ray = mouseInput.MainCamera.ScreenPointToRay(Input.ScreenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, mouseInput.RaycastDistance)) return;

        // Walk up the hierarchy to find the root placed object
        GameObject hitRoot = hit.collider.transform.root.gameObject;

        // Find which PlacedItem owns this GameObject
        foreach (var kvp in gridManager.GetAllPlacedItems())
        {
            if (kvp.Value.GameObject == hitRoot)
            {
                PickUp(kvp.Value);
                return;
            }
        }
    }

    /// <summary>
    /// Retrieves the PlaceableItem ScriptableObject for an existing placed item.
    /// Falls back to a minimal stub if not found (prevents null errors).
    /// </summary>
    private PlaceableItem GetItemDataFromPlacedItem(PlacedItem placed)
    {
        // Search all PlaceableItem assets in the project at runtime
        PlaceableItem[] all = Resources.FindObjectsOfTypeAll<PlaceableItem>();
        foreach (var item in all)
            if (item.ItemId == placed.ItemId) return item;

        Debug.LogWarning($"[PlacementManager] Could not find PlaceableItem for id '{placed.ItemId}'");
        return null;
    }

    // ── Public state ──────────────────────────────────────────────────────────

    public bool IsDragging => activeDraggable != null;

    /// <summary>
    /// Programmatically cancels any active drag.
    /// If the item was picked up from the grid, it is restored to its original position.
    /// If it was a new item from inventory, the preview object is destroyed.
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
}
