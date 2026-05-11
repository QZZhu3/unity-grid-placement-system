using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Notification button that appears when one or more chests are available to open.
///
/// Architecture: this button fires an event (<see cref="OnOpenRequested"/>) and
/// does NOT directly reference or open ChestOpeningPanel. ChestUIController
/// listens to this event and coordinates the panel.
///
/// Setup:
///   - Attach to a Button GameObject in the Canvas
///   - Assign ChestQueueManager reference (or leave empty for auto-discovery)
///   - ChestUIController will subscribe to OnOpenRequested automatically
///
/// IMPORTANT: This GameObject must be ACTIVE in the scene at startup so that
/// Awake/Start run and the queue subscription is established before any chests
/// are earned. The button hides itself visually when no chests are pending.
/// </summary>
public class ChestNotificationButton : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ChestQueueManager chestQueue;

    [Header("UI References")]
    [SerializeField] private Button           button;
    [SerializeField] private TextMeshProUGUI  badgeText;
    [SerializeField] private GameObject       badgeContainer;
    [Tooltip("Optional animator for pulse/bounce effect when a chest is available.")]
    [SerializeField] private Animator         pulseAnimator;

    [Header("Animator Parameter")]
    [Tooltip("Bool parameter name on the Animator to set true when chests are available.")]
    [SerializeField] private string pulseParam = "HasChest";

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired when the player taps the notification button.
    /// ChestUIController subscribes to this to open the panel.
    /// </summary>
    public event System.Action OnOpenRequested;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Auto-discover dependencies
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleButtonClicked);

        // Hide visually at startup — we stay active so we can receive events.
        // The actual show/hide is driven by RefreshBadge via the queue event.
        SetVisible(false);
    }

    private void Start()
    {
        // Subscribe after all Awake() calls have run (queue is guaranteed to exist).
        if (chestQueue != null)
        {
            chestQueue.OnQueueCountChanged += RefreshBadge;
            // Sync immediately in case chests were already queued before this Start ran.
            RefreshBadge(chestQueue.PendingCount);
        }
        else
        {
            Debug.LogWarning("[ChestNotificationButton] ChestQueueManager not found. " +
                             "Button will not respond to chest events.");
        }
    }

    private void OnDestroy()
    {
        if (chestQueue != null)
            chestQueue.OnQueueCountChanged -= RefreshBadge;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Manually refreshes the badge display. Called by ChestUIController after panel closes.</summary>
    public void Refresh()
    {
        if (chestQueue != null)
            RefreshBadge(chestQueue.PendingCount);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void HandleButtonClicked()
    {
        if (chestQueue == null || !chestQueue.HasPendingChests) return;
        OnOpenRequested?.Invoke();
    }

    private void RefreshBadge(int count)
    {
        bool hasChests = count > 0;

        // Show or hide the button visually (keep GameObject active for event subscription)
        SetVisible(hasChests);

        // Badge text (only shown when more than one chest is pending)
        if (badgeContainer != null)
            badgeContainer.SetActive(count > 1);

        if (badgeText != null)
            badgeText.text = count > 1 ? $"x{count}" : string.Empty;

        // Pulse animator
        if (pulseAnimator != null && pulseAnimator.isActiveAndEnabled)
            pulseAnimator.SetBool(pulseParam, hasChests);
    }

    /// <summary>
    /// Shows or hides the button visually without deactivating the GameObject.
    /// Uses CanvasGroup alpha + interactable so the subscription stays alive.
    /// </summary>
    private void SetVisible(bool visible)
    {
        // Try CanvasGroup first (preferred — keeps raycasts off when hidden)
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha          = visible ? 1f : 0f;
            cg.interactable   = visible;
            cg.blocksRaycasts = visible;
            return;
        }

        // Fallback: toggle the Button and Image components
        if (button != null)
            button.interactable = visible;

        Image img = GetComponent<Image>();
        if (img != null)
            img.enabled = visible;
    }
}
