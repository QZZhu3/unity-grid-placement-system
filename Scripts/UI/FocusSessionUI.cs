using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Presentation-only UI for the Focus Session panel.
///
/// Responsibilities:
///   - Display timer countdown
///   - Show Start / Pause / Resume / Cancel buttons
///   - Show completion popup
///   - Forward button clicks to FocusSessionRunner
///
/// This script contains NO reward logic. All rewards flow through
/// FocusSessionRunner → ActivityManager → RewardManager.
///
/// Setup:
///   Attach to a FocusSessionPanel GameObject in the Canvas.
///   Wire all serialized fields in the Inspector.
///   Assign a FocusSessionDefinition to DefaultDefinition or set it at runtime.
///
/// Hierarchy suggestion:
///   Canvas
///   └── FocusSessionPanel
///       ├── TimerText          (TextMeshProUGUI)
///       ├── SessionNameText    (TextMeshProUGUI)
///       ├── ProgressBar        (Slider, optional)
///       ├── StartButton        (Button)
///       ├── PauseResumeButton  (Button)
///       │   └── Label          (TextMeshProUGUI)
///       ├── CancelButton       (Button)
///       └── CompletionPopup    (GameObject, inactive by default)
///           ├── CompletionText (TextMeshProUGUI)
///           └── DismissButton  (Button)
/// </summary>
public class FocusSessionUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private FocusSessionRunner sessionRunner;

    [Header("Default Definition")]
    [Tooltip("The FocusSessionDefinition used when the player taps Start. " +
             "Can be overridden at runtime via SetDefinition().")]
    [SerializeField] private FocusSessionDefinition defaultDefinition;

    [Header("Timer Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private Slider          progressBar;

    [Header("Buttons")]
    [SerializeField] private Button          startButton;
    [SerializeField] private Button          pauseResumeButton;
    [SerializeField] private TextMeshProUGUI pauseResumeLabel;
    [SerializeField] private Button          cancelButton;

    [Header("Completion Popup")]
    [SerializeField] private GameObject      completionPopup;
    [SerializeField] private TextMeshProUGUI completionText;
    [SerializeField] private Button          dismissButton;

    // ── Runtime state ─────────────────────────────────────────────────────────

    private FocusSessionDefinition activeDefinition;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (sessionRunner == null)
            sessionRunner = FindAnyObjectByType<FocusSessionRunner>();

        if (sessionRunner == null)
        {
            Debug.LogError("[FocusSessionUI] FocusSessionRunner not found.");
            return;
        }

        // Wire buttons
        if (startButton       != null) startButton.onClick.AddListener(OnStartClicked);
        if (pauseResumeButton != null) pauseResumeButton.onClick.AddListener(OnPauseResumeClicked);
        if (cancelButton      != null) cancelButton.onClick.AddListener(OnCancelClicked);
        if (dismissButton     != null) dismissButton.onClick.AddListener(OnDismissClicked);

        // Subscribe to runner events
        sessionRunner.OnSessionStarted   += HandleSessionStarted;
        sessionRunner.OnSessionPaused    += HandleSessionPaused;
        sessionRunner.OnSessionResumed   += HandleSessionResumed;
        sessionRunner.OnSessionCompleted += HandleSessionCompleted;
        sessionRunner.OnSessionCancelled += HandleSessionCancelled;
        sessionRunner.OnTimerTick        += HandleTimerTick;
    }

    private void Start()
    {
        RefreshIdleState();
    }

    private void OnDestroy()
    {
        if (sessionRunner == null) return;
        sessionRunner.OnSessionStarted   -= HandleSessionStarted;
        sessionRunner.OnSessionPaused    -= HandleSessionPaused;
        sessionRunner.OnSessionResumed   -= HandleSessionResumed;
        sessionRunner.OnSessionCompleted -= HandleSessionCompleted;
        sessionRunner.OnSessionCancelled -= HandleSessionCancelled;
        sessionRunner.OnTimerTick        -= HandleTimerTick;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Override the default definition before the player taps Start.</summary>
    public void SetDefinition(FocusSessionDefinition definition)
    {
        defaultDefinition = definition;
        if (sessionNameText != null && definition != null)
            sessionNameText.text = definition.DisplayName;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnStartClicked()
    {
        FocusSessionDefinition def = defaultDefinition;
        if (def == null)
        {
            Debug.LogWarning("[FocusSessionUI] No FocusSessionDefinition assigned.");
            return;
        }
        sessionRunner.StartSession(def);
    }

    private void OnPauseResumeClicked()
    {
        if (sessionRunner.CurrentState == FocusSessionRunner.SessionState.Running)
            sessionRunner.PauseSession();
        else if (sessionRunner.CurrentState == FocusSessionRunner.SessionState.Paused)
            sessionRunner.ResumeSession();
    }

    private void OnCancelClicked()
    {
        sessionRunner.CancelSession();
    }

    private void OnDismissClicked()
    {
        if (completionPopup != null)
            completionPopup.SetActive(false);
        RefreshIdleState();
    }

    // ── Runner event handlers ─────────────────────────────────────────────────

    private void HandleSessionStarted(FocusSessionDefinition def)
    {
        activeDefinition = def;
        if (sessionNameText != null) sessionNameText.text = def.DisplayName;
        SetButtonsForRunning();
        if (completionPopup != null) completionPopup.SetActive(false);
        UpdateTimerDisplay(def.DurationSeconds);
    }

    private void HandleSessionPaused(FocusSessionDefinition def)
    {
        if (pauseResumeLabel != null) pauseResumeLabel.text = "Resume";
    }

    private void HandleSessionResumed(FocusSessionDefinition def)
    {
        if (pauseResumeLabel != null) pauseResumeLabel.text = "Pause";
    }

    private void HandleSessionCompleted(FocusSessionDefinition def)
    {
        SetButtonsForIdle();
        UpdateTimerDisplay(0f);
        ShowCompletionPopup(def);
    }

    private void HandleSessionCancelled(FocusSessionDefinition def)
    {
        RefreshIdleState();
    }

    private void HandleTimerTick(float remainingSeconds)
    {
        UpdateTimerDisplay(remainingSeconds);

        if (progressBar != null && sessionRunner != null)
            progressBar.value = sessionRunner.Progress;
    }

    // ── Display helpers ───────────────────────────────────────────────────────

    private void UpdateTimerDisplay(float seconds)
    {
        if (timerText == null) return;
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{mins:D2}:{secs:D2}";
    }

    private void ShowCompletionPopup(FocusSessionDefinition def)
    {
        if (completionPopup == null) return;
        completionPopup.SetActive(true);
        if (completionText != null)
            completionText.text = $"Session complete!\n{def?.DisplayName ?? "Focus Session"} finished.";
    }

    private void RefreshIdleState()
    {
        SetButtonsForIdle();
        FocusSessionDefinition def = defaultDefinition;
        if (sessionNameText != null)
            sessionNameText.text = def != null ? def.DisplayName : "Focus Session";
        float duration = def != null ? def.DurationSeconds : 0f;
        UpdateTimerDisplay(duration);
        if (progressBar != null) progressBar.value = 0f;
    }

    private void SetButtonsForIdle()
    {
        if (startButton       != null) startButton.gameObject.SetActive(true);
        if (pauseResumeButton != null) pauseResumeButton.gameObject.SetActive(false);
        if (cancelButton      != null) cancelButton.gameObject.SetActive(false);
    }

    private void SetButtonsForRunning()
    {
        if (startButton       != null) startButton.gameObject.SetActive(false);
        if (pauseResumeButton != null)
        {
            pauseResumeButton.gameObject.SetActive(true);
            if (pauseResumeLabel != null) pauseResumeLabel.text = "Pause";
        }
        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
    }
}
