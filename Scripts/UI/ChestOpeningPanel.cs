using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Full-screen chest opening panel with an internal state machine.
///
/// States:
///   Idle            -- panel is hidden, no chest loaded
///   OpeningAnimation -- panel fades in, chest name displayed
///   RewardReveal    -- reward slots animate in one by one
///   ResultsShown    -- all rewards visible, close button active
///   Closing         -- panel fades out
///
/// Skip behaviour:
///   First tap during RewardReveal  -> halves remaining reveal delay
///   Second tap during RewardReveal -> skips all remaining reveals instantly
///
/// This panel is opened by ChestUIController, not by ChestNotificationButton directly.
/// Reward logic (drawing items) is performed by RewardManager before Open() is called.
/// This panel only displays results.
/// </summary>
public class ChestOpeningPanel : MonoBehaviour
{
    // -- State machine ---------------------------------------------------------

    public enum PanelState { Idle, OpeningAnimation, RewardReveal, ResultsShown, Closing }

    public PanelState CurrentState { get; private set; } = PanelState.Idle;

    // -- Inspector -------------------------------------------------------------

    [Header("Panel References")]
    [SerializeField] private CanvasGroup      canvasGroup;
    [SerializeField] private GameObject       backdrop;
    [SerializeField] private TextMeshProUGUI  chestTitleText;
    [SerializeField] private Transform        rewardGrid;
    [SerializeField] private GameObject       rewardSlotPrefab;

    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button skipButton;     // optional tap-anywhere skip

    [Header("Timing")]
    [Tooltip("Duration of the panel fade-in animation.")]
    [SerializeField] private float fadeInDuration  = 0.25f;
    [Tooltip("Delay between each reward slot reveal.")]
    [SerializeField] private float revealDelay     = 0.45f;
    [Tooltip("Delay multiplier applied on first skip tap.")]
    [SerializeField] private float skipSpeedMultiplier = 0.25f;

    // -- Events ----------------------------------------------------------------

    /// <summary>Fired when the panel has fully closed and is idle again.</summary>
    public event System.Action OnPanelClosed;

    // -- Runtime state ---------------------------------------------------------

    private ChestOpenResult     currentResult;
    private List<RewardSlotUI>  spawnedSlots = new List<RewardSlotUI>();
    private Coroutine           activeCoroutine;
    private int                 skipTapCount = 0;
    private float               currentRevealDelay;

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Wire buttons
        if (openButton  != null) openButton.onClick.AddListener(OnOpenButtonClicked);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseButtonClicked);
        if (skipButton  != null) skipButton.onClick.AddListener(OnSkipTapped);

        SetState(PanelState.Idle);
    }

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Opens the panel for a chest that has already been drawn by RewardManager.
    /// Pass the ChestOpenResult directly -- no reward logic runs here.
    /// </summary>
    public void Open(ChestOpenResult result)
    {
        if (CurrentState != PanelState.Idle)
        {
            Debug.LogWarning("[ChestOpeningPanel] Open() called while not idle. Ignoring.");
            return;
        }

        currentResult = result;
        ClearSlots();
        skipTapCount = 0;
        currentRevealDelay = revealDelay;

        if (chestTitleText != null)
            chestTitleText.text = result?.Chest?.DisplayName ?? "Reward Chest";

        if (openButton  != null) openButton.gameObject.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (skipButton  != null) skipButton.gameObject.SetActive(false);

        // Activate before starting coroutine - coroutines cannot start on inactive GameObjects.
        gameObject.SetActive(true);
        SetState(PanelState.OpeningAnimation);
        activeCoroutine = StartCoroutine(FadeIn());
    }

    /// <summary>
    /// Handles a skip/tap input during reveal. First tap speeds up; second tap skips.
    /// </summary>
    public void OnSkipTapped()
    {
        if (CurrentState != PanelState.RewardReveal) return;

        skipTapCount++;

        if (skipTapCount == 1)
        {
            // First tap: speed up
            currentRevealDelay *= skipSpeedMultiplier;
            Debug.Log("[ChestOpeningPanel] Skip tap 1: reveal speed increased.");
        }
        else if (skipTapCount >= 2)
        {
            // Second tap: skip all remaining
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
                activeCoroutine = null;
            }
            SkipAllReveal();
        }
    }

    // -- State transitions -----------------------------------------------------

    private void SetState(PanelState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case PanelState.Idle:
                gameObject.SetActive(false);
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                break;

            case PanelState.OpeningAnimation:
                gameObject.SetActive(true);
                if (backdrop != null) backdrop.SetActive(true);
                break;

            case PanelState.RewardReveal:
                if (skipButton != null) skipButton.gameObject.SetActive(true);
                if (openButton != null) openButton.gameObject.SetActive(false);
                break;

            case PanelState.ResultsShown:
                if (closeButton != null) closeButton.gameObject.SetActive(true);
                if (skipButton  != null) skipButton.gameObject.SetActive(false);
                break;

            case PanelState.Closing:
                if (closeButton != null) closeButton.gameObject.SetActive(false);
                break;
        }
    }

    // -- Button handlers -------------------------------------------------------

    private void OnOpenButtonClicked()
    {
        if (CurrentState != PanelState.OpeningAnimation) return;
        SetState(PanelState.RewardReveal);
        activeCoroutine = StartCoroutine(RevealRewards());
    }

    private void OnCloseButtonClicked()
    {
        if (CurrentState != PanelState.ResultsShown) return;
        SetState(PanelState.Closing);
        activeCoroutine = StartCoroutine(FadeOut());
    }

    // -- Coroutines ------------------------------------------------------------

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        activeCoroutine = null;
        // Panel is now visible -- waiting for player to click Open
    }

    private IEnumerator RevealRewards()
    {
        if (currentResult == null || !currentResult.HasRewards)
        {
            SetState(PanelState.ResultsShown);
            yield break;
        }

        foreach (RewardResult reward in currentResult.Bundle.Items)
        {
            // Spawn slot
            RewardSlotUI slot = SpawnSlot(reward);
            slot.PlayRevealAnimation();

            // Wait for reveal delay (may be shortened by skip)
            float waited = 0f;
            while (waited < currentRevealDelay)
            {
                waited += Time.deltaTime;
                yield return null;
            }
        }

        activeCoroutine = null;
        SetState(PanelState.ResultsShown);
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeInDuration);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        activeCoroutine = null;
        SetState(PanelState.Idle);
        OnPanelClosed?.Invoke();
    }

    // -- Slot management -------------------------------------------------------

    private RewardSlotUI SpawnSlot(RewardResult reward)
    {
        GameObject go = Instantiate(rewardSlotPrefab, rewardGrid);
        RewardSlotUI slot = go.GetComponent<RewardSlotUI>();
        if (slot != null)
        {
            slot.SetReward(reward);
            spawnedSlots.Add(slot);
        }
        return slot;
    }

    private void SkipAllReveal()
    {
        // Spawn any remaining slots instantly
        if (currentResult != null && currentResult.HasRewards)
        {
            int alreadySpawned = spawnedSlots.Count;
            var items = currentResult.Bundle.Items;

            for (int i = alreadySpawned; i < items.Count; i++)
            {
                RewardSlotUI slot = SpawnSlot(items[i]);
                slot.SkipReveal();
            }

            // Skip animation on already-spawned slots
            foreach (RewardSlotUI slot in spawnedSlots)
                slot.SkipReveal();
        }

        SetState(PanelState.ResultsShown);
    }

    private void ClearSlots()
    {
        foreach (RewardSlotUI slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        spawnedSlots.Clear();
    }
}
