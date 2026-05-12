using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the visual presentation and smooth animation of the journal panel.
/// Uses soft easing for calming, cozy transitions.
///
/// Attach to: JournalPanel (child of AmbientJournalRoot)
/// Requires: RectTransform, CanvasGroup
/// </summary>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class TaskJournalPanel : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.InOut(0f, 0f, 1f, 1f);

    [Header("Positions (Anchored X on AmbientJournalRoot)")]
    [SerializeField] private float hiddenX  = -300f;
    [SerializeField] private float peekX    = -150f;
    [SerializeField] private float pinnedX  =   20f;

    private RectTransform rectTransform;
    private CanvasGroup   canvasGroup;
    private Coroutine     activeTransition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
    }

    public void TransitionToState(JournalState state, bool immediate = false)
    {
        float targetX     = hiddenX;
        float targetAlpha = 0f;
        bool  interactable = false;

        switch (state)
        {
            case JournalState.Hidden:
                targetX     = hiddenX;
                targetAlpha = 0f;
                interactable = false;
                break;
            case JournalState.Peek:
                targetX     = peekX;
                targetAlpha = 0.85f;
                interactable = true;
                break;
            case JournalState.Pinned:
                targetX     = pinnedX;
                targetAlpha = 1f;
                interactable = true;
                break;
        }

        if (immediate)
        {
            if (activeTransition != null) StopCoroutine(activeTransition);
            ApplyImmediate(targetX, targetAlpha, interactable);
            return;
        }

        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(AnimateTransition(targetX, targetAlpha, interactable));
    }

    private void ApplyImmediate(float x, float alpha, bool interactable)
    {
        rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        canvasGroup.alpha          = alpha;
        canvasGroup.interactable   = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    private IEnumerator AnimateTransition(float targetX, float targetAlpha, bool interactable)
    {
        float startX     = rectTransform.anchoredPosition.x;
        float startAlpha = canvasGroup.alpha;
        float time       = 0f;

        if (interactable)
        {
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = easeCurve.Evaluate(Mathf.Clamp01(time / transitionDuration));

            rectTransform.anchoredPosition = new Vector2(
                Mathf.Lerp(startX, targetX, t),
                rectTransform.anchoredPosition.y);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        ApplyImmediate(targetX, targetAlpha, interactable);
    }
}
