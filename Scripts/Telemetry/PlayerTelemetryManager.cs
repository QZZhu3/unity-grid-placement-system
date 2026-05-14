using System;
using System.Collections.Generic;
using UnityEngine;
using PlacementSystem.SaveSystem;

/// <summary>
/// Centralized recorder for all player behaviour events.
///
/// Architecture role:
///   - Single owner of all telemetry data.
///   - Receives events from ActivityManager, FocusSessionRunner,
///     ChestProgressManager, PlacementManager, etc.
///   - Fires OnEventRecorded so any listener (future analytics, UI) can react.
///   - Maintains a rolling recent-event history for the recommendation layer.
///   - Persists full event log and recent history via ISaveable.
///   - Has NO knowledge of UI, rewards, or gameplay logic.
///
/// How to record an event from any system:
///   PlayerTelemetryManager.Instance.Record(TelemetryEvent.TaskCompleted(instance, level));
///
/// Adding a new event type:
///   1. Add value to TelemetryEventType enum.
///   2. Add factory method to TelemetryEvent.
///   3. Call Record() from the relevant system.
///   No changes needed here.
/// </summary>
public class PlayerTelemetryManager : MonoBehaviour, ISaveable
{
    // -- Singleton -------------------------------------------------------------

    public static PlayerTelemetryManager Instance { get; private set; }

    // -- Inspector -------------------------------------------------------------

    [Header("Rolling History")]
    [Tooltip("Maximum number of recent events kept in the rolling history. " +
             "Older events are dropped. This is what the recommendation layer reads.")]
    [SerializeField, Min(10)] private int recentHistoryCapacity = 200;

    [Header("Full Log")]
    [Tooltip("Maximum number of events kept in the full persisted log. " +
             "Set to 0 for unlimited (not recommended for mobile).")]
    [SerializeField, Min(0)] private int maxLogSize = 2000;

    // -- Events ----------------------------------------------------------------

    /// <summary>
    /// Fired immediately after any event is recorded.
    /// Lightweight listeners (UI badges, streak counters) subscribe here.
    /// DO NOT perform heavy work in this callback.
    /// </summary>
    public event Action<TelemetryEvent> OnEventRecorded;

    // -- Runtime state ---------------------------------------------------------

    /// <summary>
    /// Rolling window of the most recent events.
    /// The recommendation layer reads this to understand recent player behaviour.
    /// </summary>
    private readonly Queue<TelemetryEvent> recentHistory = new Queue<TelemetryEvent>();

    /// <summary>
    /// Full event log for the current session and persisted history.
    /// </summary>
    private readonly List<TelemetryEvent> fullLog = new List<TelemetryEvent>();

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Record(TelemetryEvent.SessionStarted());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Record(TelemetryEvent.SessionEnded(Time.realtimeSinceStartup));
            Instance = null;
        }
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Record a player behaviour event.
    /// Thread-safe for the main thread. Do not call from background threads.
    /// </summary>
    public void Record(TelemetryEvent evt)
    {
        if (evt == null)
        {
            Debug.LogWarning("[Telemetry] Attempted to record null event.");
            return;
        }

        // Add to full log
        fullLog.Add(evt);
        if (maxLogSize > 0 && fullLog.Count > maxLogSize)
            fullLog.RemoveAt(0);

        // Add to rolling recent history
        recentHistory.Enqueue(evt);
        while (recentHistory.Count > recentHistoryCapacity)
            recentHistory.Dequeue();

        OnEventRecorded?.Invoke(evt);
    }

    // -- Query API (for recommendation layer) ----------------------------------

    /// <summary>
    /// Returns a snapshot of the recent event history.
    /// The recommendation provider calls this to understand player behaviour.
    /// </summary>
    public IReadOnlyList<TelemetryEvent> GetRecentHistory()
    {
        return new List<TelemetryEvent>(recentHistory);
    }

    /// <summary>
    /// Returns all events of a specific type from the recent history.
    /// </summary>
    public List<TelemetryEvent> GetRecentEventsOfType(TelemetryEventType type)
    {
        var result = new List<TelemetryEvent>();
        foreach (var evt in recentHistory)
            if (evt.eventType == type)
                result.Add(evt);
        return result;
    }

    /// <summary>
    /// Returns the count of a specific event type in the recent history.
    /// Useful for streak detection, frequency analysis, etc.
    /// </summary>
    public int CountRecentEventsOfType(TelemetryEventType type)
    {
        int count = 0;
        foreach (var evt in recentHistory)
            if (evt.eventType == type)
                count++;
        return count;
    }

    /// <summary>
    /// Returns the most recent event of a specific type, or null if none found.
    /// </summary>
    public TelemetryEvent GetMostRecentEventOfType(TelemetryEventType type)
    {
        // Iterate in reverse (most recent first)
        for (int i = fullLog.Count - 1; i >= 0; i--)
            if (fullLog[i].eventType == type)
                return fullLog[i];
        return null;
    }

    // -- ISaveable -------------------------------------------------------------

    public void PopulateSaveData(GameSaveData data)
    {
        data.telemetry = new TelemetrySaveData
        {
            recentEvents = new List<TelemetryEvent>(recentHistory),
            fullLogCount = fullLog.Count
        };
    }

    public void LoadFromSaveData(GameSaveData data)
    {
        if (data.telemetry == null) return;

        recentHistory.Clear();
        if (data.telemetry.recentEvents != null)
        {
            foreach (var evt in data.telemetry.recentEvents)
            {
                recentHistory.Enqueue(evt);
                while (recentHistory.Count > recentHistoryCapacity)
                    recentHistory.Dequeue();
            }
        }

        Debug.Log($"[Telemetry] Loaded {recentHistory.Count} recent events from save.");
    }
}

/// <summary>
/// Serializable save data for <see cref="PlayerTelemetryManager"/>.
/// Only the recent history is persisted (not the full log) to keep save files small.
/// </summary>
[Serializable]
public class TelemetrySaveData
{
    /// <summary>Rolling recent event history to restore on load.</summary>
    public List<TelemetryEvent> recentEvents = new List<TelemetryEvent>();

    /// <summary>Total number of events recorded in this save (informational only).</summary>
    public int fullLogCount = 0;
}
