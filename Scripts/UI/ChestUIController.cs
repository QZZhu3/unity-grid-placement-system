using UnityEngine;

/// <summary>
/// Mediator that coordinates the chest UI flow.
///
/// Responsibilities:
///   - Subscribes to ChestNotificationButton.OnOpenRequested
///   - Calls RewardManager.OpenNextChest() to draw rewards
///   - Passes the result to ChestOpeningPanel.Open()
///   - Blocks gameplay input via GameInputState while the panel is open
///   - Unblocks input and refreshes the notification button when the panel closes
///
/// This is the only class that couples ChestNotificationButton to ChestOpeningPanel.
/// Neither button nor panel knows about each other.
/// </summary>
public class ChestUIController : MonoBehaviour
{
    private const string InputBlockReason = "chest_ui";

    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private RewardManager           rewardManager;
    [SerializeField] private ChestNotificationButton notificationButton;
    [SerializeField] private ChestOpeningPanel       openingPanel;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (rewardManager      == null) rewardManager      = FindAnyObjectByType<RewardManager>();
        if (notificationButton == null) notificationButton = FindAnyObjectByType<ChestNotificationButton>();
        if (openingPanel       == null) openingPanel       = FindAnyObjectByType<ChestOpeningPanel>();

        if (rewardManager == null)
            Debug.LogError("[ChestUIController] RewardManager not found.");
        if (notificationButton == null)
            Debug.LogWarning("[ChestUIController] ChestNotificationButton not found.");
        if (openingPanel == null)
            Debug.LogError("[ChestUIController] ChestOpeningPanel not found.");
    }

    private void OnEnable()
    {
        if (notificationButton != null)
            notificationButton.OnOpenRequested += HandleOpenRequested;

        if (openingPanel != null)
            openingPanel.OnPanelClosed += HandlePanelClosed;
    }

    private void OnDisable()
    {
        if (notificationButton != null)
            notificationButton.OnOpenRequested -= HandleOpenRequested;

        if (openingPanel != null)
            openingPanel.OnPanelClosed -= HandlePanelClosed;
    }

    // -- Handlers --------------------------------------------------------------

    private void HandleOpenRequested()
    {
        if (rewardManager == null || openingPanel == null) return;

        // Draw the rewards now (before opening the panel)
        ChestOpenResult result = rewardManager.OpenNextChest();

        if (result == null)
        {
            Debug.LogWarning("[ChestUIController] OpenNextChest returned null. Panel will not open.");
            return;
        }

        // Block all gameplay input
        GameInputState.Block(InputBlockReason);

        // Open the panel with the pre-drawn result
        openingPanel.Open(result);

        Debug.Log("[ChestUIController] Chest panel opened. Input blocked.");
    }

    private void HandlePanelClosed()
    {
        // Unblock gameplay input
        GameInputState.Unblock(InputBlockReason);

        // Refresh the notification button badge
        if (notificationButton != null)
            notificationButton.Refresh();

        Debug.Log("[ChestUIController] Chest panel closed. Input unblocked.");
    }
}
