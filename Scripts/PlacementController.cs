using UnityEngine;

/// <summary>
/// Controls the placement system, handling item selection, preview display,
/// grid snapping via raycast, validity checking, and placement confirmation.
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

        // Disable colliders on preview
        foreach (Collider collider in previewObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        // Disable scripts on preview
        foreach (MonoBehaviour script in previewObject.GetComponentsInChildren<MonoBehaviour>())
        {
            if (script != previewComponent)
                script.enabled = false;
        }
    }

    /// <summary>
    /// Updates the preview position based on mouse position and grid snapping.
    /// </summary>
    private void UpdatePreviewPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, RaycastDistance, groundLayerMask))
        {
            // Convert hit point to grid position
            previewGridPosition = gridManager.WorldToGrid(hit.point);

            // Get the adjusted size based on rotation
            Vector2Int adjustedSize = GetAdjustedSize(selectedItem.Size, currentRotation);

            // Check if placement is valid
            canPlace = gridManager.IsAreaAvailable(previewGridPosition, adjustedSize);

            // Update preview position and validity
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
    /// Handles rotation input (R key cycles through 0°, 90°, 180°, 270°).
    /// </summary>
    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentRotation = (currentRotation + 90) % 360;
            previewObject.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    /// <summary>
    /// Handles placement input (left mouse click).
    /// </summary>
    private void HandlePlacementInput()
    {
        if (Input.GetMouseButtonDown(0) && canPlace)
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

        // Mark area as occupied
        gridManager.MarkAreaOccupied(previewGridPosition, adjustedSize);

        // Create the placed object
        Vector3 worldPosition = gridManager.GridToWorld(previewGridPosition);
        GameObject placedObject = Instantiate(selectedItem.Prefab, worldPosition, Quaternion.Euler(0, currentRotation, 0));

        // Create and register the placed item
        PlacedItem placedItem = new PlacedItem(
            selectedItem.ItemId,
            previewGridPosition,
            adjustedSize,
            currentRotation,
            placedObject
        );

        gridManager.RegisterPlacedItem(previewGridPosition, placedItem);

        // Invoke event or callback
        OnItemPlaced?.Invoke(placedItem);
    }

    /// <summary>
    /// Calculates the adjusted size based on rotation.
    /// For 90° and 270° rotations, width and height are swapped.
    /// </summary>
    private Vector2Int GetAdjustedSize(Vector2Int originalSize, int rotation)
    {
        if (rotation == 90 || rotation == 270)
        {
            return new Vector2Int(originalSize.y, originalSize.x);
        }
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

    /// <summary>
    /// Gets the currently selected item.
    /// </summary>
    public PlaceableItem GetSelectedItem() => selectedItem;

    /// <summary>
    /// Gets the current rotation angle.
    /// </summary>
    public int GetCurrentRotation() => currentRotation;

    /// <summary>
    /// Gets whether placement is currently valid.
    /// </summary>
    public bool CanPlace => canPlace;

    // Events
    public delegate void ItemPlacedDelegate(PlacedItem item);
    public delegate void ItemRemovedDelegate(PlacedItem item);

    public event ItemPlacedDelegate OnItemPlaced;
    public event ItemRemovedDelegate OnItemRemoved;
}
