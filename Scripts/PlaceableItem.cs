using UnityEngine;

/// <summary>
/// ScriptableObject that defines a placeable decoration item.
///
/// Extended from the base placement system to support the progression and
/// unlock framework. New fields are all optional — existing items without
/// a category or unlock requirement remain fully usable.
///
/// Create via: Assets → Placement System → Placeable Item
/// </summary>
[CreateAssetMenu(
    fileName = "PlaceableItem_",
    menuName = "Placement System/Placeable Item",
    order    = 1)]
public class PlaceableItem : ScriptableObject
{
    // ── Core placement data ───────────────────────────────────────────────────

    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Auto-generated from asset name if empty. " +
             "Never change after content ships — save files reference this ID.")]
    [SerializeField] private string itemId;

    [Tooltip("Human-readable name shown in the inventory UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Short description shown in item tooltips.")]
    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Tooltip("Icon shown in the inventory slot.")]
    [SerializeField] private Sprite icon;

    [Header("Placement")]
    [Tooltip("Prefab instantiated when the item is placed on the grid.")]
    [SerializeField] private GameObject prefab;

    [Tooltip("Grid footprint in cells (width × depth). Minimum 1×1.")]
    [SerializeField] private Vector2Int size = Vector2Int.one;

    // ── Progression / unlock data ─────────────────────────────────────────────

    [Header("Category & Rarity")]
    [Tooltip("The decoration category this item belongs to. " +
             "Leave null for uncategorised items — they are always available.")]
    [SerializeField] private ItemCategory category;

    [Tooltip("Rarity tier. Affects drop weight in reward pools and UI badge colour.")]
    [SerializeField] private ItemRarity rarity = ItemRarity.Common;

    [Header("Seasonal Tags")]
    [Tooltip("Season contexts this item is associated with. " +
             "Used by reward pools to surface contextually relevant items. " +
             "Leave empty for year-round items.")]
    [SerializeField] private SeasonTag[] seasonalTags = new SeasonTag[0];

    [Header("Unlock Requirement")]
    [Tooltip("Additional item-level requirement beyond category unlock. " +
             "Use this for rare or story-gated items within an already-unlocked category. " +
             "Leave null if the item is available as soon as its category is unlocked.")]
    [SerializeField] private UnlockRequirement unlockRequirement;

    // ── Public accessors ──────────────────────────────────────────────────────

    // Core
    public string      ItemId       => itemId;
    public string      DisplayName  => displayName;
    public string      Description  => description;
    public Sprite      Icon         => icon;
    public GameObject  Prefab       => prefab;
    public Vector2Int  Size         => size;

    // Progression
    public ItemCategory       Category          => category;
    public ItemRarity         Rarity            => rarity;
    public SeasonTag[]        SeasonalTags      => seasonalTags;
    public UnlockRequirement  UnlockRequirement => unlockRequirement;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if this item is tagged with the given season.
    /// </summary>
    public bool HasSeasonTag(SeasonTag tag)
    {
        if (tag == null || seasonalTags == null) return false;
        foreach (SeasonTag t in seasonalTags)
            if (t != null && t.Id == tag.Id) return true;
        return false;
    }

    // ── Editor validation ─────────────────────────────────────────────────────

    private void OnValidate()
    {
        if (size.x < 1) size.x = 1;
        if (size.y < 1) size.y = 1;

        if (string.IsNullOrEmpty(itemId))
            itemId = name.Replace("PlaceableItem_", "").ToLower().Replace(" ", "_");
    }
}
