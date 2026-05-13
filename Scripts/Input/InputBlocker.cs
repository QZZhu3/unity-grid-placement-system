using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Disables a set of MonoBehaviours and/or UI Selectables when GameInputState is blocked.
///
/// Attach one InputBlocker to each system that should be suspended during
/// full-screen overlays (chest opening, cutscenes, dialogs, etc.).
///
/// Typical setup:
///   - One InputBlocker on PlacementSystem -> targets MouseInputController, PlacementManager
///   - One InputBlocker on Canvas/InventoryPanel -> targets InventoryUI
///
/// The blocker re-enables components automatically when all blockers are cleared.
/// </summary>
public class InputBlocker : MonoBehaviour
{
    // -- Inspector -------------------------------------------------------------

    [Header("Behaviours to disable while input is blocked")]
    [Tooltip("MonoBehaviour components to enable/disable (e.g. MouseInputController, PlacementManager).")]
    [SerializeField] private List<MonoBehaviour> blockedBehaviours = new List<MonoBehaviour>();

    [Header("UI Selectables to disable while input is blocked")]
    [Tooltip("UI Selectable components to interactable=false (e.g. InventoryUI buttons).")]
    [SerializeField] private List<Selectable> blockedSelectables = new List<Selectable>();

    [Header("GameObjects to hide while input is blocked")]
    [Tooltip("Optional: GameObjects to SetActive(false) during block (e.g. inventory panel).")]
    [SerializeField] private List<GameObject> hiddenDuringBlock = new List<GameObject>();

    // -- Lifecycle -------------------------------------------------------------

    private void OnEnable()
    {
        GameInputState.OnInputBlockChanged += HandleInputBlockChanged;

        // Sync immediately in case block state was set before this component enabled.
        HandleInputBlockChanged(GameInputState.IsInputBlocked);
    }

    private void OnDisable()
    {
        GameInputState.OnInputBlockChanged -= HandleInputBlockChanged;
    }

    // -- Handler ---------------------------------------------------------------

    private void HandleInputBlockChanged(bool isBlocked)
    {
        foreach (MonoBehaviour behaviour in blockedBehaviours)
        {
            if (behaviour != null)
                behaviour.enabled = !isBlocked;
        }

        foreach (Selectable selectable in blockedSelectables)
        {
            if (selectable != null)
                selectable.interactable = !isBlocked;
        }

        foreach (GameObject go in hiddenDuringBlock)
        {
            if (go != null)
                go.SetActive(!isBlocked);
        }
    }
}
