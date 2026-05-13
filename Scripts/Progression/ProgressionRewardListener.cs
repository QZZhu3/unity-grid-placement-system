using UnityEngine;

/// <summary>
/// Listens to <see cref="RewardManager.OnRewardGranted"/> and applies XP to
/// <see cref="PlayerProgressionManager"/>.
///
/// This is the only script that calls PlayerProgressionManager.AddXp() in response
/// to task completion. Keeping this logic here means RewardManager has no knowledge
/// of XP or levelling, and PlayerProgressionManager has no knowledge of tasks.
///
/// Attach to: ProgressionSystem (alongside RewardManager and PlayerProgressionManager).
/// </summary>
public class ProgressionRewardListener : MonoBehaviour
{
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private RewardManager            rewardManager;
    [SerializeField] private PlayerProgressionManager progressionManager;

    private void Awake()
    {
        if (rewardManager       == null) rewardManager       = FindAnyObjectByType<RewardManager>();
        if (progressionManager  == null) progressionManager  = FindAnyObjectByType<PlayerProgressionManager>();

        if (rewardManager == null)
            Debug.LogWarning("[ProgressionRewardListener] RewardManager not found.");
        if (progressionManager == null)
            Debug.LogWarning("[ProgressionRewardListener] PlayerProgressionManager not found.");
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
        if (progressionManager == null) return;
        if (xp > 0f)
            progressionManager.AddXp(xp);
    }
}
