using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlacementSystem.Journal
{
    /// <summary>
    /// UI representation of a single task in the Ambient Task Journal.
    /// Adapts its display based on the current JournalState (Peek vs Pinned).
    /// </summary>
    public class TaskRowUI : MonoBehaviour
    {
        [Header("Common References")]
        [SerializeField] private Image taskIcon;
        [SerializeField] private TextMeshProUGUI taskTitle;

        [Header("Peek Mode Elements")]
        [SerializeField] private GameObject peekContainer;
        [SerializeField] private Image tinyProgressIndicator;

        [Header("Pinned Mode Elements")]
        [SerializeField] private GameObject pinnedContainer;
        [SerializeField] private TextMeshProUGUI categoryLabel;
        [SerializeField] private Image rewardPreviewImage;
        [SerializeField] private HoldCompleteInteraction holdInteraction;

        private ActivityDefinition currentTask;
        private ActivityManager activityManager;

        public void Initialize(ActivityDefinition task, ActivityManager manager)
        {
            currentTask = task;
            activityManager = manager;

            taskTitle.text = task.DisplayName;
            // categoryLabel.text = task.CategoryName; // Assuming category exists in future expansion

            if (holdInteraction != null)
            {
                holdInteraction.ResetState();
                holdInteraction.OnHoldCompleted.RemoveAllListeners();
                holdInteraction.OnHoldCompleted.AddListener(OnTaskCompleted);
            }
        }

        public void UpdateDisplayState(JournalState state)
        {
            if (peekContainer != null) peekContainer.SetActive(state == JournalState.Peek);
            if (pinnedContainer != null) pinnedContainer.SetActive(state == JournalState.Pinned);

            // Hide entirely if hidden, though usually the parent panel handles this
            gameObject.SetActive(state != JournalState.Hidden);
        }

        private void OnTaskCompleted()
        {
            if (currentTask != null && activityManager != null)
            {
                // Route completion through the central ActivityManager
                activityManager.CompleteActivity(currentTask);

                // Optional: Play local completion animation before destroying/recycling row
                Destroy(gameObject, 0.5f); // Simple cleanup for now
            }
        }
    }
}
