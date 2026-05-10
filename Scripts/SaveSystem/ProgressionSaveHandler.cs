using UnityEngine;
using System.Collections.Generic;
using PlacementSystem.SaveSystem;

/// <summary>
/// Saves and restores the state of <see cref="PlayerProgressionManager"/> and
/// <see cref="UnlockManager"/> via the shared <see cref="GameSaveData"/> schema.
///
/// All data is serialized as stable string IDs only — never as direct asset references
/// or asset names. This means assets can be renamed or moved without breaking save files.
///
/// Attach this component to the same GameObject as <see cref="PlayerProgressionManager"/>
/// and <see cref="UnlockManager"/>. <see cref="SaveManager"/> will discover it automatically
/// via the <see cref="ISaveable"/> interface.
///
/// Load order note:
///   Progression state is loaded first (level, XP, milestones), then unlock state is
///   restored. After loading, <see cref="UnlockManager.EvaluateUnlocks"/> is called to
///   process any requirements that may now be satisfied due to the restored progression.
/// </summary>
public class ProgressionSaveHandler : MonoBehaviour, ISaveable
{
    [Header("Dependencies")]
    [SerializeField] private PlayerProgressionManager progressionManager;
    [SerializeField] private UnlockManager            unlockManager;
    [SerializeField] private UnlockRewardGranter      rewardGranter;

    private void Awake()
    {
        if (progressionManager == null)
            progressionManager = GetComponent<PlayerProgressionManager>();
        if (unlockManager == null)
            unlockManager = GetComponent<UnlockManager>();
        if (rewardGranter == null)
            rewardGranter = GetComponent<UnlockRewardGranter>();
    }

    // ── ISaveable ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes progression and unlock state into <paramref name="data"/> using IDs only.
    /// </summary>
    public void PopulateSaveData(GameSaveData data)
    {
        // ── Progression ──────────────────────────────────────────────────────
        if (progressionManager != null)
        {
            var (level, xp, milestoneIds) = progressionManager.GetSaveState();
            data.progressionData.level = level;
            data.progressionData.xp    = xp;

            data.progressionData.achievedMilestoneIds.Clear();
            data.progressionData.achievedMilestoneIds.AddRange(milestoneIds);
        }

        // ── Unlock state ─────────────────────────────────────────────────────
        if (unlockManager != null)
        {
            var (categoryIds, themeIds) = unlockManager.GetSaveState();

            data.unlockData.unlockedCategoryIds.Clear();
            data.unlockData.unlockedCategoryIds.AddRange(categoryIds);

            data.unlockData.unlockedThemeIds.Clear();
            data.unlockData.unlockedThemeIds.AddRange(themeIds);
        }
    }

    /// <summary>
    /// Restores progression and unlock state from <paramref name="data"/>.
    /// Calls <see cref="UnlockManager.EvaluateUnlocks"/> after loading to process
    /// any requirements that are satisfied by the restored state.
    /// </summary>
    public void LoadFromSaveData(GameSaveData data)
    {
        // ── Progression (must load before unlock evaluation) ─────────────────
        if (progressionManager != null)
        {
            progressionManager.LoadState(
                data.progressionData.level,
                data.progressionData.xp,
                data.progressionData.achievedMilestoneIds);
        }

        // ── Unlock state ─────────────────────────────────────────────────────
        if (unlockManager != null)
        {
            // Mark previously-unlocked categories as already granted BEFORE
            // loading state and re-evaluating, so UnlockRewardGranter does not
            // re-grant items the player already received in a previous session.
            if (rewardGranter != null)
                rewardGranter.MarkCategoriesAsAlreadyGranted(
                    data.unlockData.unlockedCategoryIds);

            unlockManager.LoadState(
                data.unlockData.unlockedCategoryIds,
                data.unlockData.unlockedThemeIds);

            // Re-evaluate in case any requirements are now satisfied that were
            // not when the save was created (e.g. new content added post-ship).
            unlockManager.EvaluateUnlocks();
        }
    }
}
