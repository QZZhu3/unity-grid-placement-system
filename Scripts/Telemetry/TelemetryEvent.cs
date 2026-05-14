using System;
using UnityEngine;

/// <summary>
/// Identifies the type of a recorded player behaviour event.
///
/// Adding a new event type:
///   1. Add a value here.
///   2. Create a corresponding <see cref="TelemetryEvent"/> factory method if needed.
///   3. Call <see cref="PlayerTelemetryManager.Record"/> from the relevant system.
///   Do NOT add UI code to this file.
/// </summary>
public enum TelemetryEventType
{
    // -- Task events -----------------------------------------------------------
    TaskCompleted,
    TaskSkipped,
    TaskRerolled,
    TaskGenerated,

    // -- Focus session events --------------------------------------------------
    FocusSessionStarted,
    FocusSessionCompleted,
    FocusSessionFailed,
    FocusSessionPaused,
    FocusSessionResumed,

    // -- Chest / reward events -------------------------------------------------
    ChestOpened,
    RewardGranted,

    // -- Placement events ------------------------------------------------------
    ItemPlaced,
    ItemReturned,
    ItemRotated,

    // -- Session events --------------------------------------------------------
    SessionStarted,
    SessionEnded,

    // -- Progression events ----------------------------------------------------
    LevelUp,
    MilestoneAchieved,
}

/// <summary>
/// A single recorded player behaviour event.
///
/// Design principles:
///   - Immutable after creation (all fields set in constructor).
///   - Serializable for save/load.
///   - Carries only lightweight primitive data (IDs, counts, timestamps).
///   - Never holds asset references or MonoBehaviour pointers.
///
/// The <see cref="payload"/> dictionary is intentionally omitted to keep
/// Unity's JsonUtility compatibility. Use the typed fields instead.
/// A future version may switch to Newtonsoft.Json for richer payloads.
/// </summary>
[Serializable]
public class TelemetryEvent
{
    // -- Core fields -----------------------------------------------------------

    /// <summary>Type of event recorded.</summary>
    public TelemetryEventType eventType;

    /// <summary>UTC Unix timestamp (seconds) when the event occurred.</summary>
    public long timestampUtc;

    // -- Optional context fields (set only when relevant) ---------------------

    /// <summary>RuntimeId of the TaskInstance involved, if any.</summary>
    public string taskRuntimeId = "";

    /// <summary>Stable ID of the ActivityDefinition involved, if any.</summary>
    public string activityDefinitionId = "";

    /// <summary>Stable ID of the category involved, if any.</summary>
    public string categoryId = "";

    /// <summary>Stable ID of the chest definition involved, if any.</summary>
    public string chestDefinitionId = "";

    /// <summary>Stable ID of the item involved, if any.</summary>
    public string itemId = "";

    /// <summary>Numeric value relevant to this event (XP amount, session duration, etc.).</summary>
    public float numericValue = 0f;

    /// <summary>Player level at the time of the event.</summary>
    public int playerLevel = 0;

    /// <summary>
    /// Free-form tag for additional context.
    /// e.g. "streak_aligned", "random", "reroll_limit_reached"
    /// </summary>
    public string tag = "";

    // -- Factory methods -------------------------------------------------------

    public static TelemetryEvent TaskCompleted(TaskInstance instance, int playerLevel)
        => new TelemetryEvent
        {
            eventType            = TelemetryEventType.TaskCompleted,
            timestampUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            taskRuntimeId        = instance?.RuntimeId ?? "",
            activityDefinitionId = instance?.DefinitionId ?? "",
            playerLevel          = playerLevel,
            tag                  = instance != null && instance.SourceTags.Length > 0
                                       ? instance.SourceTags[0] : ""
        };

    public static TelemetryEvent TaskSkipped(TaskInstance instance, int playerLevel)
        => new TelemetryEvent
        {
            eventType            = TelemetryEventType.TaskSkipped,
            timestampUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            taskRuntimeId        = instance?.RuntimeId ?? "",
            activityDefinitionId = instance?.DefinitionId ?? "",
            numericValue         = instance?.SkipCount ?? 0,
            playerLevel          = playerLevel
        };

    public static TelemetryEvent TaskRerolled(TaskInstance instance, int playerLevel)
        => new TelemetryEvent
        {
            eventType            = TelemetryEventType.TaskRerolled,
            timestampUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            taskRuntimeId        = instance?.RuntimeId ?? "",
            activityDefinitionId = instance?.DefinitionId ?? "",
            numericValue         = instance?.RerollCount ?? 0,
            playerLevel          = playerLevel
        };

    public static TelemetryEvent FocusSessionCompleted(string definitionId, float durationSeconds, int playerLevel)
        => new TelemetryEvent
        {
            eventType            = TelemetryEventType.FocusSessionCompleted,
            timestampUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            activityDefinitionId = definitionId ?? "",
            numericValue         = durationSeconds,
            playerLevel          = playerLevel
        };

    public static TelemetryEvent FocusSessionFailed(string definitionId, float remainingSeconds, int playerLevel)
        => new TelemetryEvent
        {
            eventType            = TelemetryEventType.FocusSessionFailed,
            timestampUtc         = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            activityDefinitionId = definitionId ?? "",
            numericValue         = remainingSeconds,
            playerLevel          = playerLevel
        };

    public static TelemetryEvent ChestOpened(string chestId, int playerLevel)
        => new TelemetryEvent
        {
            eventType        = TelemetryEventType.ChestOpened,
            timestampUtc     = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            chestDefinitionId = chestId ?? "",
            playerLevel      = playerLevel
        };

    public static TelemetryEvent ItemPlaced(string itemId, int playerLevel)
        => new TelemetryEvent
        {
            eventType    = TelemetryEventType.ItemPlaced,
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            itemId       = itemId ?? "",
            playerLevel  = playerLevel
        };

    public static TelemetryEvent SessionStarted()
        => new TelemetryEvent
        {
            eventType    = TelemetryEventType.SessionStarted,
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

    public static TelemetryEvent SessionEnded(float sessionDurationSeconds)
        => new TelemetryEvent
        {
            eventType    = TelemetryEventType.SessionEnded,
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            numericValue = sessionDurationSeconds
        };

    public static TelemetryEvent LevelUp(int newLevel)
        => new TelemetryEvent
        {
            eventType    = TelemetryEventType.LevelUp,
            timestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            playerLevel  = newLevel
        };
}
