using UnityEngine;
using PlacementSystem.SaveSystem;

/// <summary>
/// Integrates the Focus Session system with the existing ISaveable save pipeline.
///
/// Persists:
///   - Active session definition ID
///   - Remaining time
///   - Paused state
///   - Streak placeholder fields
///
/// Requires GameSaveData to have a focusSessionData field (see setup notes).
///
/// Attach to: ProgressionSystem (alongside FocusSessionRunner)
/// </summary>
public class FocusSaveHandler : MonoBehaviour, ISaveable
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private FocusSessionRunner sessionRunner;

    [Tooltip("All FocusSessionDefinition assets that can be active at save time. " +
             "Used to resolve IDs on load. Drag all your definition assets here.")]
    [SerializeField] private FocusSessionDefinition[] allDefinitions = new FocusSessionDefinition[0];

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (sessionRunner == null)
            sessionRunner = FindAnyObjectByType<FocusSessionRunner>();
    }

    // -- ISaveable -------------------------------------------------------------

    public void PopulateSaveData(GameSaveData data)
    {
        if (sessionRunner == null) return;

        // GetSaveData returns null when no session is active -- that's fine.
        data.focusSessionData = sessionRunner.GetSaveData();
    }

    public void LoadFromSaveData(GameSaveData data)
    {
        if (sessionRunner == null) return;
        if (data.focusSessionData == null) return;

        sessionRunner.LoadFromSaveData(
            data.focusSessionData,
            id => System.Array.Find(allDefinitions, d => d.Id == id));
    }
}
