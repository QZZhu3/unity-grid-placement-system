using UnityEngine;

/// <summary>
/// Creates and configures DraggableItem instances for the placement system.
///
/// Responsibilities:
///   - Instantiating new GameObjects from PlaceableItem prefabs
///   - Adding and initialising DraggableItem components
///   - Resolving PlaceableItem ScriptableObjects from placed item IDs
///   - Disabling colliders during drag and re-enabling them after placement
///
/// This class owns no placement state. It only creates objects.
/// PlacementManager calls into PlacementFactory and owns the resulting DraggableItem.
///
/// Attach to: PlacementSystem (alongside PlacementManager).
/// </summary>
public class PlacementFactory : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private GridManager          gridManager;
    [SerializeField] private PlacementValidator   validator;
    [SerializeField] private MouseInputController mouseInput;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (gridManager == null) gridManager = FindAnyObjectByType<GridManager>();
        if (validator   == null) validator   = FindAnyObjectByType<PlacementValidator>();
        if (mouseInput  == null) mouseInput  = FindAnyObjectByType<MouseInputController>();
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Creates a new DraggableItem from a PlaceableItem prefab (for new placements from inventory).
    /// </summary>
    public DraggableItem CreateFromItem(PlaceableItem itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("[PlacementFactory] CreateFromItem called with null itemData.");
            return null;
        }

        GameObject go = Object.Instantiate(itemData.Prefab);
        return SetupDraggable(go, itemData, null);
    }

    /// <summary>
    /// Attaches a DraggableItem to an existing placed object (for pick-up / move operations).
    /// </summary>
    public DraggableItem AttachToDraggable(PlacedItem placed)
    {
        if (placed == null || placed.GameObject == null)
        {
            Debug.LogWarning("[PlacementFactory] AttachToDraggable called with null placed item.");
            return null;
        }

        PlaceableItem itemData = ResolveItemData(placed.ItemId);
        return SetupDraggable(placed.GameObject, itemData, placed.GridPosition);
    }

    /// <summary>
    /// Re-enables colliders on a placed object after drag is complete.
    /// </summary>
    public void RestoreColliders(GameObject go)
    {
        if (go == null) return;
        foreach (Collider col in go.GetComponentsInChildren<Collider>())
            col.enabled = true;
    }

    // -- Private ---------------------------------------------------------------

    private DraggableItem SetupDraggable(GameObject go, PlaceableItem itemData, Vector2Int? originalPos)
    {
        // Ensure PlacementPreview exists
        if (go.GetComponent<PlacementPreview>() == null)
            go.AddComponent<PlacementPreview>();

        // Disable colliders so raycasts pass through to the ground during drag
        foreach (Collider col in go.GetComponentsInChildren<Collider>())
            col.enabled = false;

        DraggableItem draggable = go.AddComponent<DraggableItem>();
        draggable.Initialise(itemData, gridManager, validator, mouseInput, originalPos);

        return draggable;
    }

    private PlaceableItem ResolveItemData(string itemId)
    {
        PlaceableItem[] all = Resources.FindObjectsOfTypeAll<PlaceableItem>();
        foreach (var item in all)
            if (item.ItemId == itemId) return item;

        Debug.LogWarning($"[PlacementFactory] Could not find PlaceableItem for id '{itemId}'");
        return null;
    }
}
