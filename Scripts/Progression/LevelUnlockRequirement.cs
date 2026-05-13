using UnityEngine;

/// <summary>
/// Unlock requirement satisfied when the player reaches a minimum level.
///
/// Create via: Assets -> Placement System -> Progression -> Unlock Requirement -> Level
/// </summary>
[CreateAssetMenu(
    fileName  = "Unlock_Level_",
    menuName  = "Placement System/Progression/Unlock Requirement/Level",
    order     = 20)]
public class LevelUnlockRequirement : UnlockRequirement
{
    [Tooltip("The minimum player level required to satisfy this requirement.")]
    [SerializeField] [Min(1)] private int requiredLevel = 1;

    public int RequiredLevel => requiredLevel;

    public override bool IsSatisfied(PlayerProgressionManager progression)
    {
        if (progression == null) return false;
        return progression.CurrentLevel >= requiredLevel;
    }

    public override string GetDescription()
    {
        return $"Reach Level {requiredLevel}";
    }
}
