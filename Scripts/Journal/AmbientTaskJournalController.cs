using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central state machine and input handler for the Ambient Task Journal.
/// Handles transitions between Hidden, Peek, and Pinned states based on user input.
///
/// Desktop input:
///   - Hover near edge (PeekZone) → Peek (after peekDelay seconds)
///   - Click journal during Peek → Pinned
///   - Click outside journal during Pinned → Hidden
///
/// Mobile hooks:
///   - OnMobileEdgeSwipe() → Peek
///   - OnMobileTap() → Pinned
///
/// Attach to: AmbientJournalRoot
/// </summary>
public class AmbientTaskJournalController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskJournalPanel journalPanel;
    [SerializeField] private JournalBlurController blurController;

    [Header("Settings")]
    [Tooltip("Delay in seconds before Peek mode activates to prevent flickering.")]
    [SerializeField] private float peekDelay = 0.25f;

    private JournalState currentState = JournalState.Hidden;
    private float hoverTimer = 0f;
    private bool isHoveringPeekZone = false;
    private bool isHoveringJournal = false;

    public JournalState CurrentState => currentState;

    private void Start()
    {
        if (journalPanel == null) journalPanel = GetComponentInChildren<TaskJournalPanel>();
        if (blurController == null) blurController = Object.FindAnyObjectByType<JournalBlurController>();

        SetState(JournalState.Hidden, immediate: true);
    }

    private void Update()
    {
        HandleStateTransitions();
    }

    private void HandleStateTransitions()
    {
        switch (currentState)
        {
            case JournalState.Hidden:
                if (isHoveringPeekZone)
                {
                    hoverTimer += Time.deltaTime;
                    if (hoverTimer >= peekDelay)
                        SetState(JournalState.Peek);
                }
                else
                {
                    hoverTimer = 0f;
                }
                break;

            case JournalState.Peek:
                if (!isHoveringPeekZone && !isHoveringJournal)
                {
                    SetState(JournalState.Hidden);
                }
                else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && isHoveringJournal)
                {
                    SetState(JournalState.Pinned);
                }
                break;

            case JournalState.Pinned:
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !isHoveringJournal)
                    SetState(JournalState.Hidden);
                break;
        }
    }

    private void SetState(JournalState newState, bool immediate = false)
    {
        if (currentState == newState && !immediate) return;

        JournalState oldState = currentState;
        currentState = newState;

        if (journalPanel != null)
            journalPanel.TransitionToState(newState, immediate);

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

    // ── EventTrigger hooks (wire these in the Inspector) ─────────────────────

    public void OnPeekZoneEnter()  => isHoveringPeekZone = true;
    public void OnPeekZoneExit()   => isHoveringPeekZone = false;
    public void OnJournalEnter()   => isHoveringJournal  = true;
    public void OnJournalExit()    => isHoveringJournal  = false;

    // ── Future Mobile Hooks ───────────────────────────────────────────────────

    public void OnMobileEdgeSwipe()
    {
        if (currentState == JournalState.Hidden) SetState(JournalState.Peek);
    }

    public void OnMobileTap()
    {
        if (currentState == JournalState.Peek) SetState(JournalState.Pinned);
    }
}
