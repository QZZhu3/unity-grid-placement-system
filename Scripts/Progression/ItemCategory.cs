using UnityEngine;

/// <summary>
/// Defines a decoration category (e.g. "Water Features", "Stone Paths", "Sakura Lanterns").
/// Categories are the primary unlock unit — unlocking a category makes all items in that
/// category eligible to appear in reward pools.
///
/// Categories support an optional <see cref="parentCategory"/> reference for future
/// hierarchical organisation (e.g. "Lanterns" → parent: "Lighting" → parent: "Garden Basics").
/// The hierarchy is purely informational at this stage; unlock logic operates per-category.
///
/// Create via: Assets → Placement System → Progression → Item Category
/// </summary>
[CreateAssetMenu(
    fileName = "Category_",
    menuName = "Placement System/Progression/Item Category",
    order    = 10)]
public class ItemCategory : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Never change after content ships. " +
             "Example: \"water\", \"sakura_lanterns\", \"stone_paths\".")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name shown in the UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Short flavour description shown when the category is unlocked.")]
    [TextArea(2, 4)]
    [SerializeField] private string description;

    [Tooltip("Icon shown in the unlock notification and category filter UI.")]
    [SerializeField] private Sprite icon;

    [Header("Hierarchy")]
    [Tooltip("Optional parent category for future hierarchical filtering. " +
             "Leave null for top-level categories. Does not affect unlock logic.")]
    [SerializeField] private ItemCategory parentCategory;

    [Header("Availability")]
    [Tooltip("If true, this category is available from the start without any unlock requirement. " +
             "Use for core/basic categories that should never be gated.")]
    [SerializeField] private bool isUnlockedByDefault = false;

    [Header("Theme")]
    [Tooltip("Optional theme this category belongs to. " +
             "Unlocking the parent theme will also unlock this category. " +
             "Leave null for standalone categories.")]
    [SerializeField] private DecorationTheme theme;

    // ── Public accessors ──────────────────────────────────────────────────────

    public string          Id                   => id;
    public string          DisplayName          => displayName;
    public string          Description          => description;
    public Sprite          Icon                 => icon;
    public bool            IsUnlockedByDefault  => isUnlockedByDefault;
    public ItemCategory    ParentCategory       => parentCategory;
    public DecorationTheme Theme                => theme;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name.Replace("Category_", "").ToLower().Replace(" ", "_");
    }
}
