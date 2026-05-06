using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the UI for the placement system, including item selection buttons,
/// rotation display, and placement feedback.
/// </summary>
public class PlacementUIManager : MonoBehaviour
{
    [SerializeField] private PlacementController placementController;
    [SerializeField] private Transform itemButtonContainer;
    [SerializeField] private Button itemButtonPrefab;
    [SerializeField] private TextMeshProUGUI rotationDisplay;
    [SerializeField] private TextMeshProUGUI statusDisplay;
    [SerializeField] private PlaceableItem[] availableItems;

    private Button selectedItemButton;

    private void Start()
    {
        if (placementController == null)
            placementController = FindFirstObjectByType<PlacementController>();

        CreateItemButtons();
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// Creates UI buttons for each available item.
    /// </summary>
    private void CreateItemButtons()
    {
        foreach (PlaceableItem item in availableItems)
        {
            Button button = Instantiate(itemButtonPrefab, itemButtonContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = item.DisplayName;

            button.onClick.AddListener(() => SelectItem(item, button));
        }
    }

    /// <summary>
    /// Selects an item and updates button highlighting.
    /// </summary>
    private void SelectItem(PlaceableItem item, Button button)
    {
        placementController.SelectItem(item);

        // Update button highlighting
        if (selectedItemButton != null)
            selectedItemButton.GetComponent<Image>().color = Color.white;

        selectedItemButton = button;
        button.GetComponent<Image>().color = Color.cyan;
    }

    /// <summary>
    /// Updates UI elements based on current state.
    /// </summary>
    private void UpdateUI()
    {
        // Update rotation display
        if (rotationDisplay != null)
        {
            int rotation = placementController.GetCurrentRotation();
            rotationDisplay.text = $"Rotation: {rotation}°";
        }

        // Update status display
        if (statusDisplay != null)
        {
            PlaceableItem selected = placementController.GetSelectedItem();
            if (selected == null)
            {
                statusDisplay.text = "Select an item to place";
            }
            else if (placementController.CanPlace)
            {
                statusDisplay.text = $"<color=green>Ready to place {selected.DisplayName}</color>";
            }
            else
            {
                statusDisplay.text = $"<color=red>Cannot place here</color>";
            }
        }
    }

    /// <summary>
    /// Deselects the current item.
    /// </summary>
    public void DeselectItem()
    {
        placementController.DeselectItem();

        if (selectedItemButton != null)
        {
            selectedItemButton.GetComponent<Image>().color = Color.white;
            selectedItemButton = null;
        }
    }
}
