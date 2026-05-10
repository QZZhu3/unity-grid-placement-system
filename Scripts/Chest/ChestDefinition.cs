using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data-driven definition of a chest type.
///
/// Each chest is fully described by this ScriptableObject — no code changes are
/// needed to create new chest types (seasonal, event, premium, etc.).
///
/// Create via: Assets → Placement System → Chest → Chest Definition
/// </summary>
[CreateAssetMenu(
    fileName = "Chest_",
    menuName = "Placement System/Chest/Chest Definition",
    order    = 20)]
public class ChestDefinition : ScriptableObject
{
    // ── Identity ──────────────────────────────────────────────────────────────
    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Never change after content ships. " +
             "Example: \"basic_chest\", \"sakura_chest\".")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name shown in the UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Optional description shown in the chest opening UI.")]
    [SerializeField, TextArea(2, 4)] private string description;

    [Tooltip("Icon displayed in the chest queue and opening UI.")]
    [SerializeField] private Sprite icon;

    // ── Rarity weights ────────────────────────────────────────────────────────
    [Header("Rarity Weights")]
    [Tooltip("Override rarity weights for this chest. Leave empty to use ItemRewardPool defaults.")]
    [SerializeField] private List<ChestRarityWeight> rarityWeights = new List<ChestRarityWeight>();

    // ── Seasonal filtering ────────────────────────────────────────────────────
    [Header("Seasonal Filtering")]
    [Tooltip("If set, only items tagged with one of these seasons are eligible. " +
             "Leave empty to allow all seasons.")]
    [SerializeField] private SeasonTag[] allowedSeasonTags = new SeasonTag[0];

    [Tooltip("If true, items with no season tag are also eligible alongside seasonal items.")]
    [SerializeField] private bool allowNonSeasonalItems = true;

    // ── Reward configuration ──────────────────────────────────────────────────
    [Header("Reward Configuration")]
    [Tooltip("How many items are rewarded when this chest is opened.")]
    [SerializeField, Min(1)] private int rewardsPerChest = 1;

    [Tooltip("If true, recently received items are less likely to appear again. " +
             "Requires recency tracking to be implemented.")]
    [SerializeField] private bool useRecencyDecay = false;

    [Tooltip("If true, items the player already owns are less likely to appear. " +
             "Requires duplicate protection to be implemented.")]
    [SerializeField] private bool useDuplicateProtection = false;

    // ── Audio ─────────────────────────────────────────────────────────────────
    [Header("Audio")]
    [Tooltip("Sound played when this chest is opened. Optional.")]
    [SerializeField] private AudioClip openSound;

    // ── Public accessors ──────────────────────────────────────────────────────
    public string       Id                      => id;
    public string       DisplayName             => displayName;
    public string       Description             => description;
    public Sprite       Icon                    => icon;
    public IReadOnlyList<ChestRarityWeight> RarityWeights => rarityWeights;
    public SeasonTag[]  AllowedSeasonTags        => allowedSeasonTags;
    public bool         AllowNonSeasonalItems    => allowNonSeasonalItems;
    public int          RewardsPerChest          => rewardsPerChest;
    public bool         UseRecencyDecay          => useRecencyDecay;
    public bool         UseDuplicateProtection   => useDuplicateProtection;
    public AudioClip    OpenSound                => openSound;

    /// <summary>
    /// Returns true if this chest has custom rarity weights defined.
    /// When false, the caller should fall back to ItemRewardPool defaults.
    /// </summary>
    public bool HasCustomRarityWeights => rarityWeights != null && rarityWeights.Count > 0;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name.Replace("Chest_", "").ToLower().Replace(" ", "_");
    }
}

/// <summary>
/// A rarity tier and its associated drop weight for a specific chest.
/// </summary>
[System.Serializable]
public class ChestRarityWeight
{
    [Tooltip("The rarity tier this weight applies to.")]
    public ItemRarity Rarity;

    [Tooltip("Relative weight. Higher = more likely. Example: Common=60, Uncommon=25, Rare=12, Seasonal=3.")]
    [Min(0f)]
    public float Weight = 10f;
}
