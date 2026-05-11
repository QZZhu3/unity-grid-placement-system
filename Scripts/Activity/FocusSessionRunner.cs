using System.Collections;
using UnityEngine;

/// <summary>
/// Runs a single Focus Session: manages the countdown timer, handles
/// start/pause/cancel, and signals ActivityManager on completion.
///
/// State machine:
///   Idle → Running → Paused → Running → Completed
///                          ↘ Cancelled
///
/// This class contains no reward logic. On completion it calls
/// ActivityManager.CompleteActivity(definition), which routes to RewardManager.
///
/// Attach to: ProgressionSystem (alongside ActivityManager)
/// </summary>
public class FocusSessionRunner : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ActivityManager activityManager;

    // ── State ─────────────────────────────────────────────────────────────────

    public enum SessionState { Idle, Running, Paused, Completed, Cancelled }

    public SessionState CurrentState { get; private set; } = SessionState.Idle;

    /// <summary>The definition currently running (null when Idle).</summary>
    public FocusSessionDefinition CurrentDefinition { get; private set; }

    /// <summary>Remaining time in seconds.</summary>
    public float RemainingSeconds { get; private set; }

    /// <summary>Total duration of the current session in seconds.</summary>
    public float TotalSeconds { get; private set; }

    /// <summary>Progress from 0 (just started) to 1 (complete).</summary>
    public float Progress => TotalSeconds > 0f
        ? Mathf.Clamp01(1f - RemainingSeconds / TotalSeconds)
        : 0f;

    // ── Events ────────────────────────────────────────────────────────────────

    public event System.Action<FocusSessionDefinition>        OnSessionStarted;
    public event System.Action<FocusSessionDefinition>        OnSessionPaused;
    public event System.Action<FocusSessionDefinition>        OnSessionResumed;
    public event System.Action<FocusSessionDefinition>        OnSessionCompleted;
    public event System.Action<FocusSessionDefinition>        OnSessionCancelled;
    /// <summary>Fired every frame while running. Arg: remaining seconds.</summary>
    public event System.Action<float>                         OnTimerTick;

    // ── Private ───────────────────────────────────────────────────────────────

    private Coroutine timerCoroutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (activityManager == null)
            activityManager = FindAnyObjectByType<ActivityManager>();

        if (activityManager == null)
            Debug.LogError("[FocusSessionRunner] ActivityManager not found.");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts a new focus session. Ignored if a session is already running.
    /// </summary>
    public void StartSession(FocusSessionDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogWarning("[FocusSessionRunner] StartSession called with null definition.");
            return;
        }

        if (CurrentState == SessionState.Running || CurrentState == SessionState.Paused)
        {
            Debug.LogWarning("[FocusSessionRunner] A session is already in progress. " +
                             "Cancel it before starting a new one.");
            return;
        }

        CurrentDefinition = definition;
        TotalSeconds      = definition.DurationSeconds;
        RemainingSeconds  = TotalSeconds;

        SetState(SessionState.Running);
        timerCoroutine = StartCoroutine(TimerRoutine());

        Debug.Log($"[FocusSessionRunner] Session started: '{definition.DisplayName}' " +
                  $"({definition.DurationMinutes} min)");
        OnSessionStarted?.Invoke(definition);
    }

    /// <summary>Pauses the running timer. Ignored if not running.</summary>
    public void PauseSession()
    {
        if (CurrentState != SessionState.Running) return;
        SetState(SessionState.Paused);
        OnSessionPaused?.Invoke(CurrentDefinition);
        Debug.Log("[FocusSessionRunner] Session paused.");
    }

    /// <summary>Resumes a paused session. Ignored if not paused.</summary>
    public void ResumeSession()
    {
        if (CurrentState != SessionState.Paused) return;
        SetState(SessionState.Running);
        OnSessionResumed?.Invoke(CurrentDefinition);
        Debug.Log("[FocusSessionRunner] Session resumed.");
    }

    /// <summary>Cancels the current session without granting rewards.</summary>
    public void CancelSession()
    {
        if (CurrentState == SessionState.Idle) return;

        StopTimerCoroutine();
        FocusSessionDefinition def = CurrentDefinition;
        SetState(SessionState.Cancelled);
        CurrentDefinition = null;

        Debug.Log("[FocusSessionRunner] Session cancelled.");
        OnSessionCancelled?.Invoke(def);
    }

    // ── Save / Load support ───────────────────────────────────────────────────

    /// <summary>
    /// Returns a snapshot of the current session for persistence.
    /// Returns null if no session is active.
    /// </summary>
    public FocusSessionSaveData GetSaveData()
    {
        if (CurrentState == SessionState.Idle ||
            CurrentState == SessionState.Completed ||
            CurrentState == SessionState.Cancelled)
            return null;

        return new FocusSessionSaveData
        {
            definitionId     = CurrentDefinition?.Id ?? "",
            remainingSeconds = RemainingSeconds,
            totalSeconds     = TotalSeconds,
            isPaused         = CurrentState == SessionState.Paused,
        };
    }

    /// <summary>
    /// Restores a session from saved data.
    /// Requires a resolver to look up the FocusSessionDefinition by ID.
    /// </summary>
    public void LoadFromSaveData(FocusSessionSaveData data,
                                  System.Func<string, FocusSessionDefinition> resolver)
    {
        if (data == null || string.IsNullOrEmpty(data.definitionId)) return;

        FocusSessionDefinition def = resolver(data.definitionId);
        if (def == null)
        {
            Debug.LogWarning($"[FocusSessionRunner] Could not resolve definition '{data.definitionId}' on load.");
            return;
        }

        CurrentDefinition = def;
        TotalSeconds      = data.totalSeconds;
        RemainingSeconds  = data.remainingSeconds;

        if (data.isPaused)
        {
            SetState(SessionState.Paused);
            OnSessionStarted?.Invoke(def); // notify UI to show paused state
        }
        else
        {
            SetState(SessionState.Running);
            timerCoroutine = StartCoroutine(TimerRoutine());
            OnSessionStarted?.Invoke(def);
        }

        Debug.Log($"[FocusSessionRunner] Session restored: '{def.DisplayName}' " +
                  $"({RemainingSeconds:F0}s remaining, paused={data.isPaused})");
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private IEnumerator TimerRoutine()
    {
        while (RemainingSeconds > 0f)
        {
            // Pause: wait until resumed
            while (CurrentState == SessionState.Paused)
                yield return null;

            // Cancelled mid-coroutine
            if (CurrentState == SessionState.Cancelled)
                yield break;

            RemainingSeconds -= Time.deltaTime;
            RemainingSeconds  = Mathf.Max(0f, RemainingSeconds);
            OnTimerTick?.Invoke(RemainingSeconds);
            yield return null;
        }

        // Timer reached zero — session complete
        CompleteSession();
    }

    private void CompleteSession()
    {
        FocusSessionDefinition def = CurrentDefinition;
        SetState(SessionState.Completed);
        CurrentDefinition = null;

        Debug.Log($"[FocusSessionRunner] Session completed: '{def?.DisplayName}'");
        OnSessionCompleted?.Invoke(def);

        // Route rewards through ActivityManager → RewardManager
        if (activityManager != null && def != null)
            activityManager.CompleteActivity(def);
    }

    private void StopTimerCoroutine()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void SetState(SessionState newState)
    {
        CurrentState = newState;
    }
}
