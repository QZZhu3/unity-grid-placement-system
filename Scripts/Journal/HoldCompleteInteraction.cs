using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlacementSystem.Journal
{
    /// <summary>
    /// Implements a cozy hold-to-complete interaction.
    /// Replaces standard checkbox clicks to prevent accidental completion
    /// and add intentionality to task finishing.
    /// </summary>
    public class HoldCompleteInteraction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [Tooltip("How long the button must be held to complete the task.")]
        [SerializeField] private float requiredHoldTime = 0.6f;

        [Header("UI Feedback")]
        [Tooltip("Image that fills up radially as the user holds.")]
        [SerializeField] private Image progressFillImage;
        [Tooltip("Optional animator for scale/pulse feedback.")]
        [SerializeField] private Animator feedbackAnimator;

        [Header("Events")]
        public UnityEvent OnHoldCompleted;

        private bool isHolding = false;
        private float holdTimer = 0f;
        private bool alreadyCompleted = false;

        private void Start()
        {
            if (progressFillImage != null)
            {
                progressFillImage.type = Image.Type.Filled;
                progressFillImage.fillMethod = Image.FillMethod.Radial360;
                progressFillImage.fillAmount = 0f;
            }
        }

        private void Update()
        {
            if (!isHolding || alreadyCompleted) return;

            holdTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(holdTimer / requiredHoldTime);

            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = progress;
            }

            if (holdTimer >= requiredHoldTime)
            {
                CompleteHold();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (alreadyCompleted) return;
            isHolding = true;
            holdTimer = 0f;

            if (feedbackAnimator != null)
            {
                feedbackAnimator.SetBool("IsHolding", true);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (alreadyCompleted) return;
            isHolding = false;

            // Reset if not completed
            if (progressFillImage != null)
            {
                progressFillImage.fillAmount = 0f;
            }

            if (feedbackAnimator != null)
            {
                feedbackAnimator.SetBool("IsHolding", false);
            }
        }

        private void CompleteHold()
        {
            alreadyCompleted = true;
            isHolding = false;

            if (feedbackAnimator != null)
            {
                feedbackAnimator.SetTrigger("Complete");
            }

            OnHoldCompleted?.Invoke();
        }

        /// <summary>
        /// Resets the interaction state (e.g. when reusing the row for a new task).
        /// </summary>
        public void ResetState()
        {
            alreadyCompleted = false;
            isHolding = false;
            holdTimer = 0f;
            if (progressFillImage != null) progressFillImage.fillAmount = 0f;
        }
    }
}
