using System;
using UnityEngine;

/// <summary>
/// Configures how much reward an activity grants when completed.
///
/// All values are multipliers applied on top of RewardManager's base rates.
/// Setting a multiplier to 0 disables that reward type for this activity.
///
/// Serialized inline inside ActivityDefinition -- not a ScriptableObject.
/// </summary>
[Serializable]
public class ActivityRewardConfig
{
    [Tooltip("Multiplier applied to RewardManager.xpPerTask. " +
             "1 = full XP, 0.5 = half XP, 0 = no XP.")]
    [SerializeField, Min(0f)] public float xpMultiplier = 1f;

    [Tooltip("Number of chest progress ticks granted on completion. " +
             "0 = no chest progress. Fractional values are not supported.")]
    [SerializeField, Min(0)] public int chestProgressTicks = 1;

    [Tooltip("Optional tag passed to RewardManager.CompleteTask for analytics / " +
             "future seasonal event filtering.")]
    [SerializeField] public string sourceTag = "";
}
