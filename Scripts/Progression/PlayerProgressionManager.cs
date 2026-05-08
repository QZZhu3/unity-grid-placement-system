using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player level, experience points, and progression milestones.
///
/// This class is responsible only for tracking progression state and firing events.
/// It has no knowledge of unlocks, items, or UI — those systems subscribe to its events.
///
/// XP and level-up curve:
///   XP required for each level is defined by <see cref="xpCurve"/>. If the curve has
///   fewer keys than the current level, the last defined value is used (flat cap).
///
/// Milestones:
///   <see cref="ProgressionMilestone"/> assets are evaluated after every XP gain and
///   level-up. A milestone fires <see cref="OnMilestoneAchieved"/> the first time its
///   condition is met, and is then marked as achieved.
///
/// Events:
///   <see cref="OnXpGained"/>         — fired on every XP gain
///   <see cref="OnLevelUp"/>          — fired when the player levels up
///   <see cref="OnMilestoneAchieved"/> — fired the first time a milestone is reached
/// </summary>
public class PlayerProgressionManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Starting State")]
    [SerializeField] private int   startingLevel = 1;
    [SerializeField] private float startingXp    = 0f;

    [Header("XP Curve")]
    [Tooltip("XP required to reach each level. " +
             "Index 0 = XP to reach level 2, index 1 = XP to reach level 3, etc. " +
             "If the player exceeds the last entry, that value repeats.")]
    [SerializeField] private List<float> xpCurve = new List<float>
    {
        100f,   // level 1 → 2
        200f,   // level 2 → 3
        350f,   // level 3 → 4
        550f,   // level 4 → 5
        800f,   // level 5 → 6
        1100f,  // level 6 → 7
        1500f,  // level 7 → 8
        2000f,  // level 8 → 9
        2600f,  // level 9 → 10
        3300f   // level 10+ (repeats)
    };

    [Header("Max Level")]
    [Tooltip("Set to 0 for no cap.")]
    [SerializeField] private int maxLevel = 0;

    [Header("Milestones")]
    [Tooltip("All ProgressionMilestone assets to evaluate. " +
             "Order does not matter — all are checked after every XP gain.")]
    [SerializeField] private List<ProgressionMilestone> milestones
        = new List<ProgressionMilestone>();

    // ── Runtime state ─────────────────────────────────────────────────────────

    private int   currentLevel;
    private float currentXp;
    private HashSet<string> achievedMilestoneIds = new HashSet<string>();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fired whenever XP is gained. Args: (newTotalXp).</summary>
    public event System.Action<float> OnXpGained;

    /// <summary>Fired when the player levels up. Args: (newLevel).</summary>
    public event System.Action<int> OnLevelUp;

    /// <summary>Fired the first time a milestone is achieved. Args: (milestone).</summary>
    public event System.Action<ProgressionMilestone> OnMilestoneAchieved;

    // ── Public accessors ──────────────────────────────────────────────────────

    public int   CurrentLevel => currentLevel;
    public float CurrentXp   => currentXp;

    /// <summary>XP required to reach the next level from the current level.</summary>
    public float XpToNextLevel => GetXpRequiredForLevel(currentLevel);

    /// <summary>0–1 progress toward the next level.</summary>
    public float LevelProgress =>
        XpToNextLevel > 0 ? Mathf.Clamp01(currentXp / XpToNextLevel) : 1f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        currentLevel = Mathf.Max(1, startingLevel);
        currentXp    = Mathf.Max(0f, startingXp);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Awards XP to the player. Handles level-ups and milestone checks automatically.
    /// </summary>
    public void AddXp(float amount)
    {
        if (amount <= 0f) return;
        if (maxLevel > 0 && currentLevel >= maxLevel) return;

        currentXp += amount;
        OnXpGained?.Invoke(currentXp);

        // Level-up loop (multiple level-ups possible from a single XP gain)
        while (true)
        {
            if (maxLevel > 0 && currentLevel >= maxLevel) break;

            float required = GetXpRequiredForLevel(currentLevel);
            if (currentXp < required) break;

            currentXp -= required;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
        }

        EvaluateMilestones();
    }

    /// <summary>
    /// Returns true if the given milestone has been achieved.
    /// </summary>
    public bool IsMilestoneAchieved(ProgressionMilestone milestone)
    {
        if (milestone == null) return false;
        return achievedMilestoneIds.Contains(milestone.Id);
    }

    /// <summary>
    /// Directly marks a milestone as achieved (e.g. from a story event or debug tool).
    /// Fires <see cref="OnMilestoneAchieved"/> if not already achieved.
    /// </summary>
    public void AchieveMilestone(ProgressionMilestone milestone)
    {
        if (milestone == null) return;
        if (achievedMilestoneIds.Contains(milestone.Id)) return;

        achievedMilestoneIds.Add(milestone.Id);
        OnMilestoneAchieved?.Invoke(milestone);
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    /// <summary>Restores progression state from saved data.</summary>
    public void LoadState(int level, float xp, IEnumerable<string> achievedMilestones)
    {
        currentLevel = Mathf.Max(1, level);
        currentXp    = Mathf.Max(0f, xp);
        achievedMilestoneIds = new HashSet<string>(achievedMilestones);
    }

    /// <summary>Returns a snapshot of progression state for saving.</summary>
    public (int level, float xp, string[] milestones) GetSaveState()
    {
        string[] ids = new string[achievedMilestoneIds.Count];
        achievedMilestoneIds.CopyTo(ids);
        return (currentLevel, currentXp, ids);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private float GetXpRequiredForLevel(int level)
    {
        if (xpCurve == null || xpCurve.Count == 0) return 100f;

        // level 1 → index 0, level 2 → index 1, etc.
        int index = Mathf.Clamp(level - 1, 0, xpCurve.Count - 1);
        return Mathf.Max(1f, xpCurve[index]);
    }

    private void EvaluateMilestones()
    {
        foreach (ProgressionMilestone milestone in milestones)
        {
            if (milestone == null) continue;
            if (achievedMilestoneIds.Contains(milestone.Id)) continue;
            if (!milestone.IsConditionMet(this)) continue;

            achievedMilestoneIds.Add(milestone.Id);
            OnMilestoneAchieved?.Invoke(milestone);
        }
    }

    // ── Debug ─────────────────────────────────────────────────────────────────

    [ContextMenu("Debug: Add 100 XP")]
    private void DebugAdd100Xp() => AddXp(100f);

    [ContextMenu("Debug: Print State")]
    private void DebugPrintState()
    {
        Debug.Log($"[Progression] Level {currentLevel} | XP {currentXp:F0}/{XpToNextLevel:F0} " +
                  $"({LevelProgress * 100:F1}%) | Milestones: {achievedMilestoneIds.Count}");
    }
}
