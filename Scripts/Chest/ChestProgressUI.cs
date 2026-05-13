using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the current task progress toward earning the next chest.
///
/// Shows a progress bar and a "X / Y tasks" label. Also shows a
/// "Chest ready!" indicator when a chest is waiting to be opened.
///
/// Attach to: Any UI GameObject in the HUD.
/// Wire up: ChestProgressManager (auto-discovered), progress bar Slider,
///          progress label TMP_Text, and chest-ready indicator GameObject.
/// </summary>
public class ChestProgressUI : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------
    [Header("Dependencies (auto-discovered if left empty)")]
    [SerializeField] private ChestProgressManager chestProgress;
    [SerializeField] private ChestQueueManager    chestQueue;

    [Header("UI References")]
    [SerializeField] private Slider    progressBar;
    [SerializeField] private TMP_Text  progressLabel;
    [SerializeField] private GameObject chestReadyIndicator;

    // -- Lifecycle -------------------------------------------------------------
    private void Awake()
    {
        if (chestProgress == null)
            chestProgress = FindAnyObjectByType<ChestProgressManager>();
        if (chestQueue == null)
            chestQueue = FindAnyObjectByType<ChestQueueManager>();
    }

    private void OnEnable()
    {
        if (chestProgress != null)
            chestProgress.OnProgressChanged += HandleProgressChanged;
        if (chestQueue != null)
            chestQueue.OnQueueCountChanged += HandleQueueChanged;

        // Refresh immediately
        if (chestProgress != null)
            HandleProgressChanged(chestProgress.CurrentProgress, chestProgress.TasksPerChest);
        if (chestQueue != null)
            HandleQueueChanged(chestQueue.PendingCount);
    }

    private void OnDisable()
    {
        if (chestProgress != null)
            chestProgress.OnProgressChanged -= HandleProgressChanged;
        if (chestQueue != null)
            chestQueue.OnQueueCountChanged -= HandleQueueChanged;
    }

    // -- Event handlers --------------------------------------------------------

    private void HandleProgressChanged(int current, int total)
    {
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = total;
            progressBar.value    = current;
        }

        if (progressLabel != null)
            progressLabel.text = $"{current} / {total}";
    }

    private void HandleQueueChanged(int pendingCount)
    {
        if (chestReadyIndicator != null)
            chestReadyIndicator.SetActive(pendingCount > 0);
    }
}
