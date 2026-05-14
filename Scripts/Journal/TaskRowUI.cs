using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI representation of a single task in the Ambient Task Journal.
/// Adapts its display based on the current JournalState (Peek vs Pinned).
///
/// Architecture:
///   - Receives a <see cref="TaskInstance"/> (not a raw ActivityDefinition).
///   - Reads display data from instance.Definition (static data).
///   - On completion, calls ActivityManager.CompleteActivity(TaskInstance)
///     so the recommendation layer receives full runtime metadata.
///   - Has NO knowledge of TaskGenerator, telemetry, or reward logic.
/// </summary>
public class TaskRowUI : MonoBehaviour
{
    [Header("Common References")]
    [SerializeField] private Image             taskIcon;
    [SerializeField] private TextMeshProUGUI   taskTitle;

    [Header("Peek Mode Elements")]
    [SerializeField] private GameObject        peekContainer;
    [SerializeField] private Image             tinyProgressIndicator;

    [Header("Pinned Mode Elements")]
    [SerializeField] private GameObject        pinnedContainer;
    [SerializeField] private TextMeshProUGUI   categoryLabel;
    [SerializeField] private Image             rewardPreviewImage;
    [SerializeField] private HoldCompleteInteraction holdInteraction;

    // -- Runtime state ---------------------------------------------------------

    private TaskInstance    currentInstance;
    private ActivityManager activityManager;

    // -- Public API ------------------------------------------------------------

    /// <summary>
    /// Bind this row to a TaskInstance.
    /// Called by the Journal panel when TaskGenerator fires OnTaskGenerated.
    /// </summary>
    public void Initialize(TaskInstance instance, ActivityManager manager)
    {
        currentInstance = instance;
        activityManager = manager;

        ActivityDefinition def = instance?.Definition;
        if (def == null)
        {
            Debug.LogWarning("[TaskRowUI] Initialize called with instance that has no Definition.");
            return;
        }

        if (taskTitle != null)
            taskTitle.text = def.DisplayName;

        if (categoryLabel != null)
            categoryLabel.text = def.ActivityType.ToString();

        if (holdInteraction != null)
        {
            holdInteraction.ResetState();
            holdInteraction.OnHoldCompleted.RemoveAllListeners();
            holdInteraction.OnHoldCompleted.AddListener(OnTaskCompleted);
        }
    }

    /// <summary>
    /// Legacy overload for backward compatibility with systems that still
    /// pass an ActivityDefinition directly (e.g. FocusSessionUI).
    /// Wraps the definition in a transient TaskInstance.
    /// </summary>
    public void Initialize(ActivityDefinition task, ActivityManager manager)
    {
        if (task == null) return;
        Initialize(new TaskInstance(task), manager);
    }

    /// <summary>
    /// Update the visual layout based on the current journal state.
    /// Called by TaskJournalPanel when the state machine transitions.
    /// </summary>
    public void UpdateDisplayState(JournalState state)
    {
        if (peekContainer   != null) peekContainer.SetActive(state == JournalState.Peek);
        if (pinnedContainer != null) pinnedContainer.SetActive(state == JournalState.Pinned);

        gameObject.SetActive(state != JournalState.Hidden);
    }

    // -- Private ---------------------------------------------------------------

    private void OnTaskCompleted()
    {
        if (currentInstance == null || activityManager == null) return;

        activityManager.CompleteActivity(currentInstance);
        Destroy(gameObject, 0.5f);
    }
}
