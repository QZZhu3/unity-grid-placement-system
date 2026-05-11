using UnityEngine;

/// <summary>
/// Base ScriptableObject that describes a player activity.
///
/// Subclass this for each ActivityType that needs additional configuration.
/// For example: FocusSessionDefinition extends this with duration and ambient settings.
///
/// All activities share:
///   - A stable string ID for serialization
///   - A display name and description for UI
///   - An ActivityType for routing
///   - An ActivityRewardConfig controlling XP and chest progress multipliers
///
/// Create via: Assets → Create → Productivity Garden → Activity → Activity Definition
/// </summary>
[CreateAssetMenu(
    fileName = "NewActivityDefinition",
    menuName  = "Productivity Garden/Activity/Activity Definition")]
public class ActivityDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable string ID used for serialization. Never change after shipping.")]
    [SerializeField] private string id = "";

    [Tooltip("Human-readable name shown in UI.")]
    [SerializeField] private string displayName = "Activity";

    [Tooltip("Short description shown in UI.")]
    [TextArea(2, 4)]
    [SerializeField] private string description = "";

    [Header("Type")]
    [SerializeField] private ActivityType activityType = ActivityType.ChecklistTask;

    [Header("Rewards")]
    [SerializeField] private ActivityRewardConfig rewardConfig = new ActivityRewardConfig();

    // ── Public accessors ──────────────────────────────────────────────────────

    public string              Id          => id;
    public string              DisplayName => displayName;
    public string              Description => description;
    public ActivityType        ActivityType => activityType;
    public ActivityRewardConfig RewardConfig => rewardConfig;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name.ToLower().Replace(" ", "_");
    }
#endif
}
