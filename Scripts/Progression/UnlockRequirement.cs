using UnityEngine;

/// <summary>
/// Abstract base for all unlock requirements.
/// Subclass this to create new unlock condition types without modifying
/// any existing code (open/closed principle).
///
/// Concrete implementations:
///   <see cref="LevelUnlockRequirement"/>     — unlocks at a player level threshold
///   <see cref="MilestoneUnlockRequirement"/> — unlocks when a named milestone is achieved
/// </summary>
public abstract class UnlockRequirement : ScriptableObject
{
    /// <summary>
    /// Returns true if this requirement is currently satisfied.
    /// </summary>
    /// <param name="progression">The active PlayerProgressionManager instance.</param>
    public abstract bool IsSatisfied(PlayerProgressionManager progression);

    /// <summary>
    /// Human-readable description of what the player needs to do.
    /// Used in tooltip and unlock preview UI.
    /// </summary>
    public abstract string GetDescription();
}
