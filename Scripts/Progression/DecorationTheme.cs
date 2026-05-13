using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Groups multiple <see cref="ItemCategory"/> entries under a named aesthetic theme
/// (e.g. "Sakura Garden", "Night Garden", "Coastal Retreat").
///
/// A theme is unlocked when its <see cref="unlockRequirement"/> is satisfied.
/// Unlocking a theme automatically unlocks all of its member categories via
/// <see cref="UnlockManager"/>. Unlock logic is entirely delegated to the
/// <see cref="UnlockRequirement"/> ScriptableObject -- no level thresholds are
/// hardcoded here.
///
/// Create via: Assets -> Placement System -> Progression -> Decoration Theme
/// </summary>
[CreateAssetMenu(
    fileName = "Theme_",
    menuName = "Placement System/Progression/Decoration Theme",
    order    = 11)]
public class DecorationTheme : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Never change after content ships. " +
             "Example: \"sakura\", \"night_garden\", \"coastal\".")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name shown in the UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Flavour text shown in the unlock notification popup.")]
    [TextArea(2, 5)]
    [SerializeField] private string unlockMessage;

    [Tooltip("Splash art or banner shown in the unlock notification.")]
    [SerializeField] private Sprite splashArt;

    [Header("Categories")]
    [Tooltip("All item categories that belong to this theme. " +
             "Unlocking the theme will automatically unlock every category in this list.")]
    [SerializeField] private List<ItemCategory> categories = new List<ItemCategory>();

    [Header("Unlock")]
    [Tooltip("The requirement that must be satisfied before this theme becomes available. " +
             "Assign a LevelUnlockRequirement, MilestoneUnlockRequirement, or any custom " +
             "UnlockRequirement subclass. Leave null to make the theme available from the start.")]
    [SerializeField] private UnlockRequirement unlockRequirement;

    // -- Public accessors ------------------------------------------------------

    public string                      Id                 => id;
    public string                      DisplayName        => displayName;
    public string                      UnlockMessage      => unlockMessage;
    public Sprite                      SplashArt          => splashArt;
    public IReadOnlyList<ItemCategory> Categories         => categories;
    public UnlockRequirement           UnlockRequirement  => unlockRequirement;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name.Replace("Theme_", "").ToLower().Replace(" ", "_");
    }
}
