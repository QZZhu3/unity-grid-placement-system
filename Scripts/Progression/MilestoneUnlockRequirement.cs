using UnityEngine;

/// <summary>
/// Unlock requirement satisfied when a specific <see cref="ProgressionMilestone"/>
/// has been achieved by the player.
///
/// Create via: Assets -> Placement System -> Progression -> Unlock Requirement -> Milestone
/// </summary>
[CreateAssetMenu(
    fileName  = "Unlock_Milestone_",
    menuName  = "Placement System/Progression/Unlock Requirement/Milestone",
    order     = 21)]
public class MilestoneUnlockRequirement : UnlockRequirement
{
    [Tooltip("The milestone that must be achieved to satisfy this requirement.")]
    [SerializeField] private ProgressionMilestone requiredMilestone;

    public ProgressionMilestone RequiredMilestone => requiredMilestone;

    public override bool IsSatisfied(PlayerProgressionManager progression)
    {
        if (progression == null || requiredMilestone == null) return false;
        return progression.IsMilestoneAchieved(requiredMilestone);
    }

    public override string GetDescription()
    {
        if (requiredMilestone == null) return "Complete a milestone";
        return $"Achieve: {requiredMilestone.DisplayName}";
    }
}
