using UnityEngine;

/// <summary>
/// ScriptableObject that defines a placeable item's properties.
/// Contains item ID, prefab reference, and grid footprint size.
/// </summary>
[CreateAssetMenu(fileName = "PlaceableItem_", menuName = "Placement System/Placeable Item", order = 1)]
public class PlaceableItem : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public string ItemId => itemId;
    public GameObject Prefab => prefab;
    public Vector2Int Size => size;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    private void OnValidate()
    {
        // Ensure size is at least 1x1
        if (size.x < 1) size.x = 1;
        if (size.y < 1) size.y = 1;

        // Generate itemId from asset name if empty
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = name.Replace("PlaceableItem_", "").ToLower();
        }
    }
}
