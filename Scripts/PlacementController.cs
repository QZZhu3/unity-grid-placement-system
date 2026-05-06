using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the placement system, handling item selection, preview display,
/// grid snapping via raycast, validity checking, and placement confirmation.
/// Uses the New Input System (UnityEngine.InputSystem).
/// Supports an optional OnPlacementValidation hook for inventory quantity gating.
/// </summary>
public class PlacementController : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayerMask;

    private PlaceableItem selectedItem;
    private GameObject previewObject;
    private PlacementPreview previewComponent;
    private int currentRotation = 0; // 0, 90, 180, 270
    private Vector2Int previewGridPosition = Vector2Int.zero;
    private bool canPlace = false;

    private const float RaycastDistance = 1000f;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();
    }

    private void Update()
    {
        if (selectedItem == null)
            return;

        UpdatePreviewPosition();
        HandleRotationInput();
        HandlePlacementInput();
    }

    /// <summary>
    /// Selects an item to place and creates a preview.
    /// </summary>
    public void SelectItem(PlaceableItem item)
    {
        if (item == null)
        {
            DeselectItem();
            return;
        }

        selectedItem = item;
        currentRotation = 0;
        CreatePreview();
    }

    /// <summary>
    /// Deselects the current item and destroys the preview.
    /// </summary>
    public void DeselectItem()
    {
        selectedItem = null;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
            previewComponent = null;
        }
    }

    /// <summary>
    /// Creates the preview object from the selected item's prefab.
    /// </summary>
    private void CreatePreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = Instantiate(selectedItem.Prefab);
        previewComponent = previewObject.AddComponent<PlacementPreview>();

        // Disable colliders on preview so raycasts pass through to the ground
        foreach (Collider col in previewObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Disable other scripts on preview
        foreach (MonoBehaviour script in previewObject.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script != previewComponent)
                script.enabled = false;
        }
    }

    /// <summary>
    /// Updates the preview position based on mouse position and grid snapping.
    /// Also checks inventory quantity via the OnPlacementValidation delegate.
    /// </summary>
    private void UpdatePreviewPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, RaycastDistance, groundLayerMask))
        {
            previewGridPosition = gridManager.WorldToGrid(hit.point);
            Vector2Int adjustedSize = GetAdjustedSize(selectedItem.Size, currentRotation);

            // Check grid availability
            bool gridAvailable = gridManager.IsAreaAvailable(previewGridPosition, adjustedSize);

            // Check inventory quantity via validation hook (returns true if no hook is registered)
            bool inventoryAvailable = OnPlacementValidation == null || OnPlacementValidation.Invoke(selectedItem);

            canPlace = gridAvailable && inventoryAvailable;

            Vector3 previewWorldPosition = gridManager.GridToWorld(previewGridPosition);
            previewObject.transform.position = previewWorldPosition;
            previewComponent.SetValidity(canPlace);
        }
        else
        {
            canPlace = false;
            previewComponent.SetValidity(false);
        }
    }

    /// <summary>
    /// Handles rotation input — R key cycles through 0, 90, 180, 270 degrees.
    /// </summary>
    private void HandleRotationInput()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            currentRotation = (currentRotation + 90) % 360;
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    /// <summary>
    /// Handles placement input — left mouse button places the item.
    /// </summary>
    private void HandlePlacementInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
        {
            PlaceItem();
        }
    }

    /// <summary>
    /// Places the item on the grid and creates the final object.
    /// </summary>
    private void PlaceItem()
    {
        Vector2Int adjustedSize = GetAdjustedSize(selectedItem.Size, currentRotation);

        gridManager.MarkAreaOccupied(previewGridPosition, adjustedSize);

        Vector3 worldPosition = gridManager.GridToWorld(previewGridPosition);
        GameObject placedObject = Instantiate(selectedItem.Prefab, worldPosition, Quaternion.Euler(0, currentRotation, 0));

        PlacedItem placedItem = new PlacedItem(
            selectedItem.ItemId,
            previewGridPosition,
            adjustedSize,
            currentRotation,
            placedObject
        );

        gridManager.RegisterPlacedItem(previewGridPosition, placedItem);
        OnItemPlaced?.Invoke(placedItem);
    }

    /// <summary>
    /// Calculates the adjusted size based on rotation.
    /// For 90 and 270 degree rotations, width and height are swapped.
    /// </summary>
    private Vector2Int GetAdjustedSize(Vector2Int originalSize, int rotation)
    {
        if (rotation == 90 || rotation == 270)
            return new Vector2Int(originalSize.y, originalSize.x);
        return originalSize;
    }

    /// <summary>
    /// Removes a placed item from the grid.
    /// </summary>
    public void RemoveItem(Vector2Int gridPosition)
    {
        PlacedItem item = gridManager.GetPlacedItem(gridPosition);
        if (item != null)
        {
            gridManager.MarkAreaUnoccupied(item.GridPosition, item.Size);
            gridManager.UnregisterPlacedItem(gridPosition);
            Destroy(item.GameObject);
            OnItemRemoved?.Invoke(item);
        }
    }

    public PlaceableItem GetSelectedItem() => selectedItem;
    public int GetCurrentRotation() => currentRotation;
    public bool CanPlace => canPlace;

    // Delegates and Events
    public delegate void ItemPlacedDelegate(PlacedItem item);
    public delegate void ItemRemovedDelegate(PlacedItem item);
    public delegate bool PlacementValidationDelegate(PlaceableItem item);

    /// <summary>
    /// Fires after an item is successfully placed on the grid.
    /// </summary>
    public event ItemPlacedDelegate OnItemPlaced;

    /// <summary>
    /// Fires after an item is removed from the grid.
    /// </summary>
    public event ItemRemovedDelegate OnItemRemoved;

    /// <summary>
    /// Optional validation hook. Subscribe to add custom placement conditions
    /// (e.g., inventory quantity check). Return false to block placement.
    /// </summary>
    public PlacementValidationDelegate OnPlacementValidation;
}
