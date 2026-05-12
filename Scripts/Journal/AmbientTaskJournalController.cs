using UnityEngine;
using UnityEngine.EventSystems;

namespace PlacementSystem.Journal
{
    /// <summary>
    /// Central state machine and input handler for the Ambient Task Journal.
    /// Handles transitions between Hidden, Peek, and Pinned states based on user input.
    ///
    /// Desktop input:
    ///   - Hover near edge (PeekZone) -> Peek
    ///   - Click journal during Peek -> Pinned
    ///   - Click outside journal during Pinned -> Hidden
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
                        {
                            SetState(JournalState.Peek);
                        }
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
                    else if (Input.GetMouseButtonDown(0) && isHoveringJournal)
                    {
                        SetState(JournalState.Pinned);
                    }
                    break;

                case JournalState.Pinned:
                    // Click outside to close
                    if (Input.GetMouseButtonDown(0) && !isHoveringJournal)
                    {
                        SetState(JournalState.Hidden);
                    }
                    break;
            }
        }

        private void SetState(JournalState newState, bool immediate = false)
        {
            if (currentState == newState && !immediate) return;

            JournalState oldState = currentState;
            currentState = newState;

            // Notify UI panel
            if (journalPanel != null)
            {
                journalPanel.TransitionToState(newState, immediate);
            }

            // Handle Blur/Input blocking
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

        // ── Input Hooks (Called by EventTriggers on UI elements) ──────────────

        public void OnPeekZoneEnter() => isHoveringPeekZone = true;
        public void OnPeekZoneExit() => isHoveringPeekZone = false;

        public void OnJournalEnter() => isHoveringJournal = true;
        public void OnJournalExit() => isHoveringJournal = false;

        // Future Mobile Hooks
        public void OnMobileEdgeSwipe()
        {
            if (currentState == JournalState.Hidden) SetState(JournalState.Peek);
        }

        public void OnMobileTap()
        {
            if (currentState == JournalState.Peek) SetState(JournalState.Pinned);
        }
    }
}
