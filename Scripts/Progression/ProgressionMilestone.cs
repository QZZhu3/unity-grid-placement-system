using UnityEngine;

/// <summary>
/// A named progression milestone that can be used as an unlock gate via
/// <see cref="MilestoneUnlockRequirement"/>.
///
/// Milestones are evaluated by <see cref="PlayerProgressionManager"/> after every
/// XP gain. A milestone fires <see cref="PlayerProgressionManager.OnMilestoneAchieved"/>
/// the first time its condition is met.
///
/// Built-in condition types (set via <see cref="conditionType"/>):
///   - ReachLevel: achieved when the player reaches <see cref="requiredLevel"/>.
///   - Manual:     achieved only when explicitly triggered via
///                 <see cref="PlayerProgressionManager.AchieveMilestone"/> (story events, etc.).
///
/// Create via: Assets → Placement System → Progression → Progression Milestone
/// </summary>
[CreateAssetMenu(
    fileName = "Milestone_",
    menuName = "Placement System/Progression/Progression Milestone",
    order    = 14)]
public class ProgressionMilestone : ScriptableObject
{
    public enum ConditionType
    {
        /// <summary>Achieved when the player reaches <see cref="requiredLevel"/>.</summary>
        ReachLevel,

        /// <summary>Achieved only when triggered programmatically (story, tutorial, etc.).</summary>
        Manual
    }

    [Header("Identity")]
    [Tooltip("Stable machine-readable ID. Never change after content ships.")]
    [SerializeField] private string id;

    [Tooltip("Human-readable name shown in the UI.")]
    [SerializeField] private string displayName;

    [Tooltip("Flavour text shown when this milestone is achieved.")]
    [TextArea(2, 4)]
    [SerializeField] private string achievementMessage;

    [Header("Condition")]
    [SerializeField] private ConditionType conditionType = ConditionType.ReachLevel;

    [Tooltip("Required player level. Only used when ConditionType is ReachLevel.")]
    [SerializeField] private int requiredLevel = 5;

    [Header("XP Reward")]
    [Tooltip("XP awarded to the player when this milestone is first achieved.")]
    [SerializeField] [Min(0)] private int xpReward = 50;

    // ── Public accessors ──────────────────────────────────────────────────────

    public string        Id                 => id;
    public string        DisplayName        => displayName;
    public string        AchievementMessage => achievementMessage;
    public ConditionType Condition          => conditionType;
    public int           RequiredLevel      => requiredLevel;
    public int           XpReward           => xpReward;

    // ── Evaluation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if this milestone's automatic condition is currently satisfied.
    /// Manual milestones always return false here — they must be triggered explicitly.
    /// </summary>
    public bool IsConditionMet(PlayerProgressionManager progression)
    {
        if (progression == null) return false;

        return conditionType switch
        {
            ConditionType.ReachLevel => progression.CurrentLevel >= requiredLevel,
            ConditionType.Manual     => false,
            _                        => false
        };
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = name.Replace("Milestone_", "").ToLower().Replace(" ", "_");
        if (requiredLevel < 1) requiredLevel = 1;
    }
}
