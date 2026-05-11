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
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleButtonClicked);
    }

    private void OnEnable()
    {
        if (chestQueue != null)
        {
            chestQueue.OnQueueCountChanged += RefreshBadge;
            RefreshBadge(chestQueue.PendingCount);
        }
    }

    private void OnDisable()
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

        // Show/hide the button
        gameObject.SetActive(hasChests);

        // Badge text
        if (badgeContainer != null)
            badgeContainer.SetActive(count > 1);

        if (badgeText != null)
            badgeText.text = count > 1 ? $"x{count}" : string.Empty;

        // Pulse animator
        if (pulseAnimator != null && pulseAnimator.isActiveAndEnabled)
        {
            pulseAnimator.SetBool(pulseParam, hasChests);
        }
    }
}
