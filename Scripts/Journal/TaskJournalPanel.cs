using System.Collections;
using UnityEngine;

namespace PlacementSystem.Journal
{
    /// <summary>
    /// Handles the visual presentation and animation of the journal panel.
    /// Uses smooth easing for soft, calming transitions.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class TaskJournalPanel : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float transitionDuration = 0.35f;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.InOut(0f, 0f, 1f, 1f);

        [Header("Positions (Anchored X)")]
        [SerializeField] private float hiddenX = -300f;
        [SerializeField] private float peekX = -150f;
        [SerializeField] private float pinnedX = 20f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Coroutine activeTransition;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void TransitionToState(JournalState state, bool immediate = false)
        {
            float targetX = hiddenX;
            float targetAlpha = 0f;
            bool interactable = false;

            switch (state)
            {
                case JournalState.Hidden:
                    targetX = hiddenX;
                    targetAlpha = 0f;
                    interactable = false;
                    break;
                case JournalState.Peek:
                    targetX = peekX;
                    targetAlpha = 0.8f; // Soft opacity for ambient feel
                    interactable = true; // Needs to detect click to pin
                    break;
                case JournalState.Pinned:
                    targetX = pinnedX;
                    targetAlpha = 1f;
                    interactable = true;
                    break;
            }

            if (immediate)
            {
                if (activeTransition != null) StopCoroutine(activeTransition);
                rectTransform.anchoredPosition = new Vector2(targetX, rectTransform.anchoredPosition.y);
                canvasGroup.alpha = targetAlpha;
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
                return;
            }

            if (activeTransition != null) StopCoroutine(activeTransition);
            activeTransition = StartCoroutine(AnimateTransition(targetX, targetAlpha, interactable));
        }

        private IEnumerator AnimateTransition(float targetX, float targetAlpha, bool interactable)
        {
            float startX = rectTransform.anchoredPosition.x;
            float startAlpha = canvasGroup.alpha;
            float time = 0f;

            // Apply interaction state immediately if showing, at end if hiding
            if (interactable)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                float t = easeCurve.Evaluate(time / transitionDuration);

                rectTransform.anchoredPosition = new Vector2(
                    Mathf.Lerp(startX, targetX, t),
                    rectTransform.anchoredPosition.y
                );
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(targetX, rectTransform.anchoredPosition.y);
            canvasGroup.alpha = targetAlpha;

            if (!interactable)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
