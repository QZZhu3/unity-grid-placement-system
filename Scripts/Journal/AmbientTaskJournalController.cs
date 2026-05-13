using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central state machine for the Ambient Task Journal.
///
/// Peek detection uses screen-edge mouse proximity (no EventTrigger needed on PeekZone).
/// Hover-over-panel detection uses JournalHoverZone EventTrigger (OnJournalEnter/Exit).
/// Click-to-pin and click-outside-to-close use Mouse.current.
///
/// Desktop flow:
///   Mouse within edgeThreshold px of left edge -> Peek (after peekDelay seconds)
///   Click journal while Peeked -> Pinned
///   Click outside journal while Pinned -> Hidden
///
/// Mobile hooks (call from UI buttons):
///   OnMobileEdgeSwipe() -> Peek
///   OnMobileTap()       -> Pinned
///
/// Attach to: AmbientJournalRoot
/// </summary>
public class AmbientTaskJournalController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskJournalPanel      journalPanel;
    [SerializeField] private JournalBlurController blurController;

    [Header("Edge Detection")]
    [Tooltip("Pixels from the left edge of the screen that trigger Peek mode.")]
    [SerializeField] private float edgeThreshold = 50f;

    [Header("Settings")]
    [Tooltip("Seconds the cursor must stay near the edge before Peek activates.")]
    [SerializeField] private float peekDelay = 0.25f;

    [Header("Debug")]
    [Tooltip("Enable verbose per-frame logging in the Console.")]
    [SerializeField] private bool debugLogging = true;

    private JournalState currentState    = JournalState.Hidden;
    private float        hoverTimer      = 0f;
    private bool         isHoveringJournal = false;

    public JournalState CurrentState => currentState;

    // -- Lifecycle -------------------------------------------------------------

    private void Start()
    {
        if (journalPanel   == null) journalPanel   = GetComponentInChildren<TaskJournalPanel>(includeInactive: true);
        if (blurController == null) blurController = Object.FindAnyObjectByType<JournalBlurController>();

        // Self-test: report wiring status
        Debug.Log($"[Journal] AmbientTaskJournalController.Start() on '{gameObject.name}'" +
                  $" | journalPanel={(journalPanel != null ? journalPanel.gameObject.name : \"NULL\")}" +
                  $" | blurController={(blurController != null ? blurController.gameObject.name : \"NULL\")}" +
                  $" | edgeThreshold={edgeThreshold} | peekDelay={peekDelay}");

        if (journalPanel == null)
            Debug.LogError("[Journal] journalPanel is NULL! Attach TaskJournalPanel to JournalPanel (child of AmbientJournalRoot).");

        if (Mouse.current == null)
            Debug.LogWarning("[Journal] Mouse.current is NULL -- check Project Settings > Input System.");

        SetState(JournalState.Hidden, immediate: true);
    }

    private void Update()
    {
        HandleStateTransitions();
    }

    // -- State Machine ---------------------------------------------------------

    private void HandleStateTransitions()
    {
        bool nearEdge = IsMouseNearLeftEdge();
        float mouseX  = Mouse.current != null ? Mouse.current.position.ReadValue().x : -1f;

        if (debugLogging && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[Journal] State={currentState} | nearEdge={nearEdge} | mouseX={mouseX:F0}" +
                      $" | hoverTimer={hoverTimer:F2} | isHoveringJournal={isHoveringJournal}" +
                      $" | screenW={Screen.width}");
        }

        switch (currentState)
        {
            case JournalState.Hidden:
                if (nearEdge)
                {
                    hoverTimer += Time.deltaTime;
                    if (debugLogging && Time.frameCount % 10 == 0)
                        Debug.Log($"[Journal] Near edge -- hoverTimer={hoverTimer:F2}/{peekDelay}");
                    if (hoverTimer >= peekDelay)
                        SetState(JournalState.Peek);
                }
                else
                {
                    hoverTimer = 0f;
                }
                break;

            case JournalState.Peek:
                if (!nearEdge && !isHoveringJournal)
                {
                    SetState(JournalState.Hidden);
                }
                else if (Mouse.current != null &&
                         Mouse.current.leftButton.wasPressedThisFrame &&
                         isHoveringJournal)
                {
                    SetState(JournalState.Pinned);
                }
                break;

            case JournalState.Pinned:
                if (Mouse.current != null &&
                    Mouse.current.leftButton.wasPressedThisFrame &&
                    !isHoveringJournal)
                {
                    SetState(JournalState.Hidden);
                }
                break;
        }
    }

    private bool IsMouseNearLeftEdge()
    {
        if (Mouse.current == null) return false;
        return Mouse.current.position.ReadValue().x <= edgeThreshold;
    }

    // -- State Setter ----------------------------------------------------------

    private void SetState(JournalState newState, bool immediate = false)
    {
        if (currentState == newState && !immediate) return;

        JournalState oldState = currentState;
        currentState = newState;

        Debug.Log($"[Journal] *** STATE CHANGE: {oldState} -> {newState} (immediate={immediate}) ***");

        if (newState == JournalState.Hidden) hoverTimer = 0f;

        if (journalPanel != null)
            journalPanel.TransitionToState(newState, immediate);
        else
            Debug.LogError("[Journal] Cannot transition -- journalPanel is null!");

        if (newState == JournalState.Pinned)
        {
            if (blurController != null) blurController.SetBlurActive(true);
            GameInputState.Block("ambient_journal");
        }
        else if (oldState == JournalState.Pinned)
        {
            if (blurController != null) blurController.SetBlurActive(false);
            GameInputState.Unblock("ambient_journal");
        }
    }

    // -- EventTrigger hooks (wire on JournalHoverZone) -------------------------
    // These are only needed to keep the journal open while the cursor is over it.

    public void OnJournalEnter()
    {
        isHoveringJournal = true;
        if (debugLogging) Debug.Log("[Journal] OnJournalEnter");
    }

    public void OnJournalExit()
    {
        isHoveringJournal = false;
        if (debugLogging) Debug.Log("[Journal] OnJournalExit");
    }

    // -- Mobile Hooks ----------------------------------------------------------

    public void OnMobileEdgeSwipe()
    {
        if (currentState == JournalState.Hidden) SetState(JournalState.Peek);
    }

    public void OnMobileTap()
    {
        if (currentState == JournalState.Peek) SetState(JournalState.Pinned);
    }
}
