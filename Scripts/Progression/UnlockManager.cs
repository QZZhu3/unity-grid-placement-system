using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Single source of truth for all unlock state in the game.
///
/// All systems — reward pools, UI filters, progression milestones — MUST query
/// this manager rather than performing their own unlock checks. This ensures
/// unlock logic is never duplicated or hardcoded elsewhere.
///
/// Unlock flow:
///   PlayerProgressionManager → (OnLevelUp / OnMilestoneAchieved)
///     → UnlockManager.EvaluateUnlocks()
///       → fires OnCategoryUnlocked / OnThemeUnlocked
///         → ItemRewardPool / UI subscribe and react
///
/// Unlock rules (all data-driven via ScriptableObjects):
///   - A <see cref="DecorationTheme"/> unlocks when its <see cref="UnlockRequirement"/> is satisfied.
///     Unlocking a theme automatically unlocks all its member categories.
///   - A standalone <see cref="ItemCategory"/> (not part of a theme) unlocks when added to
///     <see cref="standaloneCategories"/> and its requirement is satisfied, or when
///     <see cref="UnlockCategory"/> is called directly (e.g. from a reward chest).
///   - A <see cref="PlaceableItem"/> is eligible when its category is unlocked AND its own
///     optional item-level <see cref="UnlockRequirement"/> is satisfied.
///   - Items with no category are always eligible (uncategorised = unrestricted).
/// </summary>
public class UnlockManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies")]
    [SerializeField] private PlayerProgressionManager progression;

    [Header("Content Registry")]
    [Tooltip("Every DecorationTheme asset in the game. " +
             "Evaluated in order; unlocking a theme unlocks all its categories.")]
    [SerializeField] private List<DecorationTheme> allThemes = new List<DecorationTheme>();

    [Tooltip("Categories that have their own unlock gate but are not part of any theme. " +
             "These are evaluated independently. A null UnlockRequirement means always unlocked.")]
    [SerializeField] private List<StandaloneCategoryEntry> standaloneCategories
        = new List<StandaloneCategoryEntry>();

    // ── Runtime state ─────────────────────────────────────────────────────────

    private HashSet<string> unlockedCategoryIds = new HashSet<string>();
    private HashSet<string> unlockedThemeIds    = new HashSet<string>();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired the first time a category becomes unlocked. Args: (category).</summary>
    public event System.Action<ItemCategory> OnCategoryUnlocked;

    /// <summary>Fired the first time a theme becomes unlocked. Args: (theme).</summary>
    public event System.Action<DecorationTheme> OnThemeUnlocked;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (progression == null)
            progression = FindAnyObjectByType<PlayerProgressionManager>();
    }

    private void Start()
    {
        if (progression != null)
        {
            progression.OnLevelUp           += _ => EvaluateUnlocks();
            progression.OnMilestoneAchieved += _ => EvaluateUnlocks();
        }

        EvaluateUnlocks();
    }

    // ── Public API — Evaluation ───────────────────────────────────────────────

    /// <summary>
    /// Re-evaluates all unlock requirements and fires events for any newly
    /// satisfied conditions. Safe to call multiple times — already-unlocked
    /// items are skipped.
    /// </summary>
    public void EvaluateUnlocks()
    {
        // 1. Themes (unlocking a theme cascades to its categories)
        foreach (DecorationTheme theme in allThemes)
        {
            if (theme == null) continue;
            if (unlockedThemeIds.Contains(theme.Id)) continue;

            bool satisfied = theme.UnlockRequirement == null
                || theme.UnlockRequirement.IsSatisfied(progression);

            if (!satisfied) continue;

            unlockedThemeIds.Add(theme.Id);
            OnThemeUnlocked?.Invoke(theme);

            foreach (ItemCategory cat in theme.Categories)
                GrantCategoryUnlock(cat);
        }

        // 2. Standalone categories
        foreach (StandaloneCategoryEntry entry in standaloneCategories)
        {
            if (entry.Category == null) continue;
            if (unlockedCategoryIds.Contains(entry.Category.Id)) continue;

            bool satisfied = entry.UnlockRequirement == null
                || entry.UnlockRequirement.IsSatisfied(progression);

            if (satisfied)
                GrantCategoryUnlock(entry.Category);
        }
    }

    // ── Public API — Direct unlock (reward chests, story events) ─────────────

    /// <summary>
    /// Directly unlocks a category regardless of requirement.
    /// Use for reward chests, story triggers, or debug tools.
    /// </summary>
    public void UnlockCategory(ItemCategory category)
    {
        if (category == null) return;
        GrantCategoryUnlock(category);
    }

    /// <summary>
    /// Directly unlocks a theme and all its member categories.
    /// </summary>
    public void UnlockTheme(DecorationTheme theme)
    {
        if (theme == null) return;
        if (!unlockedThemeIds.Contains(theme.Id))
        {
            unlockedThemeIds.Add(theme.Id);
            OnThemeUnlocked?.Invoke(theme);
        }
        foreach (ItemCategory cat in theme.Categories)
            GrantCategoryUnlock(cat);
    }

    // ── Public API — Queries (single source of truth) ─────────────────────────

    /// <summary>
    /// Returns true if the given category is currently unlocked.
    /// A null category means the item is uncategorised and always available.
    /// </summary>
    public bool IsCategoryUnlocked(ItemCategory category)
    {
        if (category == null) return true;
        return unlockedCategoryIds.Contains(category.Id);
    }

    /// <summary>
    /// Returns true if the given theme is currently unlocked.
    /// </summary>
    public bool IsThemeUnlocked(DecorationTheme theme)
    {
        if (theme == null) return true;
        return unlockedThemeIds.Contains(theme.Id);
    }

    /// <summary>
    /// Returns true if a <see cref="PlaceableItem"/> is eligible to appear in reward pools.
    ///
    /// Eligibility requires ALL of the following:
    ///   1. The item's category is unlocked (or the item has no category).
    ///   2. The item's own unlock requirement is satisfied (or it has none).
    ///
    /// This is the canonical eligibility check. All reward pools and UI filters
    /// MUST call this method instead of performing their own checks.
    /// </summary>
    public bool IsItemEligible(PlaceableItem item)
    {
        if (item == null) return false;

        if (!IsCategoryUnlocked(item.Category)) return false;

        if (item.UnlockRequirement != null && !item.UnlockRequirement.IsSatisfied(progression))
            return false;

        return true;
    }

    /// <summary>
    /// Returns true if a <see cref="PlaceableItem"/> is eligible AND matches the given season tag.
    /// Pass null for <paramref name="season"/> to skip the season filter.
    /// </summary>
    public bool IsItemEligibleForSeason(PlaceableItem item, SeasonTag season)
    {
        if (!IsItemEligible(item)) return false;
        if (season == null) return true;
        return item.HasSeasonTag(season);
    }

    /// <summary>
    /// Filters a list of items and returns only those currently eligible.
    /// Allocates a new list — cache the result if called frequently.
    /// </summary>
    public List<PlaceableItem> FilterEligible(IEnumerable<PlaceableItem> items)
    {
        List<PlaceableItem> result = new List<PlaceableItem>();
        foreach (PlaceableItem item in items)
            if (IsItemEligible(item)) result.Add(item);
        return result;
    }

    /// <summary>
    /// Filters a list of items and returns only those eligible for the given season.
    /// </summary>
    public List<PlaceableItem> FilterEligibleForSeason(
        IEnumerable<PlaceableItem> items, SeasonTag season)
    {
        List<PlaceableItem> result = new List<PlaceableItem>();
        foreach (PlaceableItem item in items)
            if (IsItemEligibleForSeason(item, season)) result.Add(item);
        return result;
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    /// <summary>Restores unlock state from saved data.</summary>
    public void LoadState(IEnumerable<string> categoryIds, IEnumerable<string> themeIds)
    {
        unlockedCategoryIds = new HashSet<string>(categoryIds);
        unlockedThemeIds    = new HashSet<string>(themeIds);
    }

    /// <summary>Returns a snapshot of unlock state for saving.</summary>
    public (string[] categories, string[] themes) GetSaveState()
    {
        string[] cats   = new string[unlockedCategoryIds.Count];
        string[] themes = new string[unlockedThemeIds.Count];
        unlockedCategoryIds.CopyTo(cats);
        unlockedThemeIds.CopyTo(themes);
        return (cats, themes);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void GrantCategoryUnlock(ItemCategory category)
    {
        if (category == null) return;
        if (unlockedCategoryIds.Contains(category.Id)) return;

        unlockedCategoryIds.Add(category.Id);
        OnCategoryUnlocked?.Invoke(category);
    }

    // ── Debug ─────────────────────────────────────────────────────────────────

    [ContextMenu("Debug: Print Unlock State")]
    private void DebugPrintState()
    {
        Debug.Log($"[UnlockManager] Categories: {string.Join(", ", unlockedCategoryIds)}");
        Debug.Log($"[UnlockManager] Themes:     {string.Join(", ", unlockedThemeIds)}");
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

/// <summary>
/// Pairs a standalone category with its own unlock requirement.
/// Used in <see cref="UnlockManager.standaloneCategories"/>.
/// </summary>
[System.Serializable]
public class StandaloneCategoryEntry
{
    [Tooltip("The category to unlock.")]
    public ItemCategory Category;

    [Tooltip("Requirement to satisfy. Leave null to unlock immediately at game start.")]
    public UnlockRequirement UnlockRequirement;
}
