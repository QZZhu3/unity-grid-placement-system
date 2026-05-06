using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single slot in the inventory UI grid.
/// Displays the item icon, name, and quantity.
/// Notifies the InventoryUI when clicked to begin placement.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectionHighlight;

    private InventorySlot slot;
    private System.Action<InventorySlot> onClickCallback;

    private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
    private Color selectedColor = new Color(0.15f, 0.5f, 0.15f, 0.9f);
    private Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);

    private bool isSelected = false;

    /// <summary>
    /// Initializes the slot UI with data and a click callback.
    /// </summary>
    public void Initialize(InventorySlot inventorySlot, System.Action<InventorySlot> onClick)
    {
        slot = inventorySlot;
        onClickCallback = onClick;
        Refresh();
    }

    /// <summary>
    /// Refreshes the displayed data from the current slot state.
    /// </summary>
    public void Refresh()
    {
        if (slot == null) return;

        bool isEmpty = slot.IsEmpty;

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = slot.Item.Icon;
            iconImage.enabled = slot.Item.Icon != null;
            iconImage.color = isEmpty ? new Color(1, 1, 1, 0.3f) : Color.white;
        }

        // Item name
        if (itemNameText != null)
        {
            itemNameText.text = slot.Item.DisplayName;
            itemNameText.color = isEmpty ? new Color(1, 1, 1, 0.4f) : Color.white;
        }

        // Quantity
        if (quantityText != null)
        {
            quantityText.text = isEmpty ? "0" : slot.Quantity.ToString();
            quantityText.color = isEmpty ? new Color(1, 0.3f, 0.3f, 1f) : new Color(1f, 0.9f, 0.3f, 1f);
        }

        // Background
        if (backgroundImage != null)
        {
            backgroundImage.color = isEmpty ? emptyColor : (isSelected ? selectedColor : normalColor);
        }
    }

    /// <summary>
    /// Sets the selected visual state of this slot.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;

        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty) return;
        onClickCallback?.Invoke(slot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && backgroundImage != null && !slot.IsEmpty)
            backgroundImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && backgroundImage != null)
            backgroundImage.color = slot.IsEmpty ? emptyColor : normalColor;
    }

    public InventorySlot Slot => slot;
}
