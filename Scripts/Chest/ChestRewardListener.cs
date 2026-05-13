using UnityEngine;

/// <summary>
/// Listens to <see cref="RewardManager.OnRewardGranted"/> and applies chest progress
/// ticks to <see cref="ChestProgressManager"/>.
///
/// This is the only script that calls ChestProgressManager.AddProgress() in response
/// to task completion. Keeping this logic here means RewardManager has no knowledge
/// of chest progress, and ChestProgressManager has no knowledge of tasks.
///
/// Attach to: ProgressionSystem (alongside RewardManager and ChestProgressManager).
/// </summary>
public class ChestRewardListener : MonoBehaviour
{
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private RewardManager      rewardManager;
    [SerializeField] private ChestProgressManager chestProgress;

    private void Awake()
    {
        if (rewardManager  == null) rewardManager  = FindAnyObjectByType<RewardManager>();
        if (chestProgress  == null) chestProgress  = FindAnyObjectByType<ChestProgressManager>();

        if (rewardManager == null)
            Debug.LogWarning("[ChestRewardListener] RewardManager not found.");
        if (chestProgress == null)
            Debug.LogWarning("[ChestRewardListener] ChestProgressManager not found.");
    }

    private void OnEnable()
    {
        if (rewardManager != null)
            rewardManager.OnRewardGranted += HandleRewardGranted;
    }

    private void OnDisable()
    {
        if (rewardManager != null)
            rewardManager.OnRewardGranted -= HandleRewardGranted;
    }

    private void HandleRewardGranted(float xp, int chestTicks)
    {
        if (chestProgress == null) return;
        if (chestTicks > 0)
            chestProgress.AddProgress(chestTicks);
    }
}
