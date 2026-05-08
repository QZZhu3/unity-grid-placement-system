using UnityEngine;

/// <summary>
/// Identifies a seasonal context for decoration items
/// (e.g. Spring, Summer, Autumn, Winter, Lunar New Year, Halloween).
///
/// Items tagged with a SeasonTag can be filtered in reward pools and UI
/// to surface contextually relevant decorations.
///
/// Using ScriptableObject references instead of raw strings ensures
/// refactor safety, Inspector autocomplete, and no typo-based bugs.
///
/// Create via: Assets → Placement System → Progression → Season Tag
/// </summary>
[CreateAssetMenu(
    fileName = "Season_",
    menuName = "Placement System/Progression/Season Tag",
    order    = 13)]
public class SeasonTag : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Never change after content ships. " +
             "Example: \"spring\", \"lunar_new_year\", \"halloween\".")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name shown in the UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Optional colour tint used when this season is active in the UI.")]
    [SerializeField] private Color accentColor = Color.white;

    [Tooltip("Optional icon representing this season.")]
    [SerializeField] private Sprite icon;

    // ── Public accessors ──────────────────────────────────────────────────────

    public string Id          => id;
    public string DisplayName => displayName;
    public Color  AccentColor => accentColor;
    public Sprite Icon        => icon;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name.Replace("Season_", "").ToLower().Replace(" ", "_");
    }
}
