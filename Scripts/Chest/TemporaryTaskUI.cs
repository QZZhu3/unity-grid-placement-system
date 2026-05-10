using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Temporary debug UI to simulate task completion.
///
/// Attach to a Button in the UI to manually trigger task completion
/// and test the chest progression flow.
/// </summary>
public class TemporaryTaskUI : MonoBehaviour
{
    [SerializeField] private Button completeTaskButton;
    [SerializeField] private RewardManager rewardManager;

    private void Awake()
    {
        if (rewardManager == null)
            rewardManager = FindAnyObjectByType<RewardManager>();

        if (completeTaskButton != null)
            completeTaskButton.onClick.AddListener(OnTaskCompleted);
        else
            Debug.LogWarning("[TemporaryTaskUI] Complete Task Button is not assigned.");
    }

    private void OnTaskCompleted()
    {
        if (rewardManager != null)
            rewardManager.CompleteTask();
        else
            Debug.LogError("[TemporaryTaskUI] RewardManager is missing.");
    }
}
