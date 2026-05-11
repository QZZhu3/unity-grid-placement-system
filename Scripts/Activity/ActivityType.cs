/// <summary>
/// Identifies the broad category of a player activity.
///
/// Used by ActivityDefinition and ActivityManager to route completion
/// signals and configure reward multipliers.
///
/// Adding a new activity type:
///   1. Add a value here.
///   2. Create a new ActivityDefinition subclass (ScriptableObject).
///   3. Handle the new type in ActivityManager if special logic is needed.
///      All reward distribution still flows through RewardManager.
/// </summary>
public enum ActivityType
{
    /// <summary>Player ticks off a to-do or checklist item.</summary>
    ChecklistTask,

    /// <summary>Player completes a timed focus / Pomodoro session.</summary>
    FocusSession,

    /// <summary>Player writes a short reflection or journal entry.</summary>
    ReflectionTask,

    /// <summary>Short optional ambient interaction (watering, lighting lanterns, etc.).</summary>
    MicroInteraction,
}
