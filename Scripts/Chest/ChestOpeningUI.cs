using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the chest opening UI panel.
///
/// Shows the chest icon, animates the opening, then reveals each reward card
/// one by one before dismissing.
///
/// This script is intentionally minimal — it drives a simple reveal sequence
/// without requiring a full animation state machine. Replace the
/// <see cref="RevealDelay"/> and <see cref="DismissDelay"/> values with
/// Animator triggers when you add proper animations.
///
/// Requires:
///   - A Canvas panel (the root of this UI) with a CanvasGroup for fade
///   - A chest icon Image
///   - A chest name TMP label
///   - A reward card container (horizontal/vertical layout group)
///   - A reward card prefab with Image + TMP label
///
/// Attach to: The root GameObject of your chest opening UI panel.
/// </summary>
public class ChestOpeningUI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Dependencies")]
    [SerializeField] private RewardManager rewardManager;

    [Header("Panel References")]
    [SerializeField] private CanvasGroup   panelGroup;
    [SerializeField] private Image         chestIcon;
    [SerializeField] private TMP_Text      chestNameLabel;
    [SerializeField] private TMP_Text      chestDescriptionLabel;
    [SerializeField] private Transform     rewardCardContainer;
    [SerializeField] private GameObject    rewardCardPrefab;
    [SerializeField] private Button        openButton;
    [SerializeField] private Button        dismissButton;
    [SerializeField] private TMP_Text      pendingCountLabel;

    [Header("Timing")]
    [Tooltip("Seconds between each reward card appearing.")]
    [SerializeField] private float revealDelay = 0.3f;

    [Tooltip("Seconds after all cards appear before auto-dismiss is available.")]
    [SerializeField] private float dismissDelay = 1.0f;

    // ── Runtime ───────────────────────────────────────────────────────────────
    private bool isDismissable;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (rewardManager == null)
            rewardManager = FindAnyObjectByType<RewardManager>();

        if (rewardManager != null)
        {
            rewardManager.OnChestOpened += HandleChestOpened;
            // Also listen for queue changes to update the open button
            ChestQueueManager queue = FindAnyObjectByType<ChestQueueManager>();
            if (queue != null)
            {
                queue.OnQueueCountChanged += UpdateOpenButton;
                UpdateOpenButton(queue.PendingCount);
            }
        }

        if (openButton    != null) openButton.onClick.AddListener(OnOpenButtonClicked);
        if (dismissButton != null) dismissButton.onClick.AddListener(OnDismissButtonClicked);

        // Start hidden
        SetPanelVisible(false);
    }

    private void OnDestroy()
    {
        if (rewardManager != null)
            rewardManager.OnChestOpened -= HandleChestOpened;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleChestOpened(ChestOpenResult result)
    {
        StopAllCoroutines();
        StartCoroutine(PlayOpenSequence(result));
    }

    private void UpdateOpenButton(int pendingCount)
    {
        if (openButton != null)
            openButton.interactable = pendingCount > 0;

        if (pendingCountLabel != null)
            pendingCountLabel.text = pendingCount > 0 ? $"x{pendingCount}" : "";
    }

    // ── Button callbacks ──────────────────────────────────────────────────────

    private void OnOpenButtonClicked()
    {
        rewardManager?.OpenNextChest();
        // HandleChestOpened will be called via the event
    }

    private void OnDismissButtonClicked()
    {
        if (!isDismissable) return;
        SetPanelVisible(false);
    }

    // ── Sequence ──────────────────────────────────────────────────────────────

    private IEnumerator PlayOpenSequence(ChestOpenResult result)
    {
        isDismissable = false;
        ClearRewardCards();
        SetPanelVisible(true);

        // Populate chest info
        if (chestIcon != null && result.Chest.Icon != null)
            chestIcon.sprite = result.Chest.Icon;

        if (chestNameLabel != null)
            chestNameLabel.text = result.Chest.DisplayName;

        if (chestDescriptionLabel != null)
            chestDescriptionLabel.text = result.Chest.Description;

        // Brief pause before revealing rewards
        yield return new WaitForSeconds(revealDelay);

        // Reveal reward cards one by one
        if (result.Bundle != null)
        {
            foreach (RewardResult reward in result.Bundle.Items)
            {
                SpawnRewardCard(reward);
                yield return new WaitForSeconds(revealDelay);
            }
        }

        // Wait before allowing dismiss
        yield return new WaitForSeconds(dismissDelay);
        isDismissable = true;

        if (dismissButton != null)
            dismissButton.interactable = true;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SpawnRewardCard(RewardResult reward)
    {
        if (rewardCardPrefab == null || rewardCardContainer == null) return;

        GameObject card = Instantiate(rewardCardPrefab, rewardCardContainer);

        // Set item icon
        Image icon = card.GetComponentInChildren<Image>();
        if (icon != null && reward.Item.Icon != null)
            icon.sprite = reward.Item.Icon;

        // Set item name + quantity label
        TMP_Text label = card.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            string qty = reward.Quantity > 1 ? $" x{reward.Quantity}" : "";
            label.text = $"{reward.Item.DisplayName}{qty}\n<size=80%>{reward.DrawnRarity}</size>";
        }
    }

    private void ClearRewardCards()
    {
        if (rewardCardContainer == null) return;
        foreach (Transform child in rewardCardContainer)
            Destroy(child.gameObject);

        if (dismissButton != null)
            dismissButton.interactable = false;
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelGroup == null) return;
        panelGroup.alpha          = visible ? 1f : 0f;
        panelGroup.interactable   = visible;
        panelGroup.blocksRaycasts = visible;
    }
}
