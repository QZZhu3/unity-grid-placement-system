using System;

/// <summary>
/// Serializable snapshot of an active Focus Session.
///
/// Stored inside GameSaveData.focusSessionData.
/// Uses string IDs only -- never asset references.
/// </summary>
[Serializable]
public class FocusSessionSaveData
{
    /// <summary>Stable ID of the FocusSessionDefinition asset.</summary>
    public string definitionId = "";

    /// <summary>Remaining time in seconds at the point of save.</summary>
    public float remainingSeconds = 0f;

    /// <summary>Total session duration in seconds (from the definition at session start).</summary>
    public float totalSeconds = 0f;

    /// <summary>Whether the session was paused when saved.</summary>
    public bool isPaused = false;

    // -- Streak placeholder ----------------------------------------------------

    /// <summary>
    /// Placeholder for future streak support.
    /// Stores the UTC date string (yyyy-MM-dd) of the last completed session.
    /// Not yet used by any system.
    /// </summary>
    public string lastCompletedDateUtc = "";

    /// <summary>
    /// Placeholder for future streak support.
    /// Current streak count (consecutive days with at least one completed session).
    /// Not yet used by any system.
    /// </summary>
    public int currentStreak = 0;
}
