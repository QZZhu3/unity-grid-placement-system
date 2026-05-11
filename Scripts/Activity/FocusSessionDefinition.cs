using UnityEngine;

/// <summary>
/// ScriptableObject that configures a Focus Session activity.
///
/// Extends ActivityDefinition with:
///   - Session duration
///   - Ambient interaction type (cosmetic only, no fail state)
///   - Streak hook placeholder
///
/// Create via: Assets → Create → Productivity Garden → Activity → Focus Session Definition
///
/// Example asset: FocusSession_Standard (25 min, WateringFlowers ambient)
/// </summary>
[CreateAssetMenu(
    fileName = "NewFocusSessionDefinition",
    menuName  = "Productivity Garden/Activity/Focus Session Definition")]
public class FocusSessionDefinition : ActivityDefinition
{
    [Header("Focus Session Settings")]
    [Tooltip("Session duration in minutes.")]
    [SerializeField, Min(1f)] private float durationMinutes = 25f;

    [Tooltip("Short break duration in minutes shown after session completes (informational only).")]
    [SerializeField, Min(0f)] private float breakMinutes = 5f;

    [Header("Ambient Interaction")]
    [Tooltip("The ambient interaction type shown during the session. " +
             "Purely cosmetic — no fail state.")]
    [SerializeField] private AmbientInteractionType ambientType = AmbientInteractionType.DriftingLeaves;

    [Tooltip("Whether the ambient interaction is shown at all during this session.")]
    [SerializeField] private bool showAmbientInteraction = true;

    [Header("Streak (future hook)")]
    [Tooltip("Whether completing this session contributes to a streak counter. " +
             "Streak logic is not yet implemented — this is a placeholder.")]
    [SerializeField] private bool countsTowardStreak = true;

    // ── Public accessors ──────────────────────────────────────────────────────

    /// <summary>Session duration in seconds (converted from durationMinutes).</summary>
    public float DurationSeconds      => durationMinutes * 60f;
    public float DurationMinutes      => durationMinutes;
    public float BreakMinutes         => breakMinutes;
    public AmbientInteractionType AmbientType => ambientType;
    public bool  ShowAmbientInteraction => showAmbientInteraction;
    public bool  CountsTowardStreak   => countsTowardStreak;
}
