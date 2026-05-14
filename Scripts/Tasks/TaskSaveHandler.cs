using UnityEngine;
using PlacementSystem.SaveSystem;

/// <summary>
/// Integrates the TaskGenerator system with the ISaveable save pipeline.
///
/// Persists:
///   - The currently active TaskInstance (so the same task is shown after reload)
///
/// On load:
///   - Restores the TaskInstance from save data
///   - Re-links the ActivityDefinition asset via DefinitionId
///   - Injects the restored instance back into TaskGenerator
///
/// Attach to: ProgressionSystem (alongside TaskGenerator)
/// </summary>
public class TaskSaveHandler : MonoBehaviour, ISaveable
{
    // -- Inspector -------------------------------------------------------------

    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private TaskGenerator taskGenerator;

    [Tooltip("All ActivityDefinition assets that may be the active task at save time. " +
             "Used to resolve DefinitionId on load. Drag all your definition assets here.")]
    [SerializeField] private ActivityDefinition[] allDefinitions = new ActivityDefinition[0];

    // -- Lifecycle -------------------------------------------------------------

    private void Awake()
    {
        if (taskGenerator == null)
            taskGenerator = FindAnyObjectByType<TaskGenerator>();
    }

    // -- ISaveable -------------------------------------------------------------

    public void PopulateSaveData(GameSaveData data)
    {
        if (taskGenerator == null) return;

        TaskInstance current = taskGenerator.GetCurrentInstance();
        data.activeTaskInstance = current?.ToSaveData();
    }

    public void LoadFromSaveData(GameSaveData data)
    {
        if (taskGenerator == null) return;
        if (data.activeTaskInstance == null) return;

        // Resolve the ActivityDefinition asset from the saved DefinitionId
        ActivityDefinition def = System.Array.Find(
            allDefinitions,
            d => d.Id == data.activeTaskInstance.definitionId);

        if (def == null)
        {
            Debug.LogWarning($"[TaskSaveHandler] Could not find ActivityDefinition with ID " +
                             $"'{data.activeTaskInstance.definitionId}'. " +
                             $"A new task will be generated instead.");
            return;
        }

        TaskInstance restored = TaskInstance.FromSaveData(data.activeTaskInstance);
        restored.Definition = def;

        // Inject the restored instance into the generator so it doesn't
        // generate a new one on Start (TaskGenerator checks this)
        taskGenerator.InjectRestoredInstance(restored);

        Debug.Log($"[TaskSaveHandler] Restored active task: '{def.DisplayName}' " +
                  $"(runtimeId: {restored.RuntimeId})");
    }
}
