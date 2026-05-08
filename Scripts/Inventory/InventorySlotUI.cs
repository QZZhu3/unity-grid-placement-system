using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single slot button in the inventory UI grid.
///
/// Displays the item icon, display name, and quantity.
/// Notifies <see cref="InventoryUI"/> when clicked/tapped to begin placement.
///
/// Mobile readiness:
///   - Implements IPointerClickHandler (works for both mouse and touch).
///   - Minimum recommended button size: 80×80 px (set via RectTransform in the prefab).
///   - Visual feedback on hover (desktop) and on selection (all platforms).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    // ── Inspector references ──────────────────────────────────────────────────

    [Header("Icon & Text")]
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Background & Highlight")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image selectionHighlight;  // optional overlay image

    // ── Colours ───────────────────────────────────────────────────────────────

    private static readonly Color NormalColor   = new Color(0.20f, 0.20f, 0.20f, 0.90f);
    private static readonly Color HoverColor    = new Color(0.30f, 0.30f, 0.30f, 0.90f);
    private static readonly Color SelectedColor = new Color(0.15f, 0.50f, 0.15f, 0.90f);
    private static readonly Color EmptyColor    = new Color(0.15f, 0.15f, 0.15f, 0.60f);

    // ── Runtime state ─────────────────────────────────────────────────────────

    private InventorySlot            slot;
    private System.Action<InventorySlot> onClickCallback;
    private bool                     isSelected;

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>The inventory data slot this UI element represents.</summary>
    public InventorySlot Slot => slot;

    /// <summary>
    /// Binds this slot UI to an <see cref="InventorySlot"/> and registers the click callback.
    /// Must be called once after instantiation.
    /// </summary>
    public void Initialize(InventorySlot inventorySlot, System.Action<InventorySlot> onClick)
    {
        slot            = inventorySlot;
        onClickCallback = onClick;
        isSelected      = false;
        Refresh();
    }

    /// <summary>
    /// Re-reads the bound <see cref="InventorySlot"/> and updates all visual elements.
    /// Call this whenever the underlying inventory quantity changes.
    /// </summary>
    public void Refresh()
    {
        if (slot == null) return;

        bool isEmpty = slot.IsEmpty;

        // ── Icon ──────────────────────────────────────────────────────────────
        if (iconImage != null)
        {
            iconImage.sprite  = slot.Item?.Icon;
            iconImage.enabled = slot.Item?.Icon != null;
            iconImage.color   = isEmpty ? new Color(1f, 1f, 1f, 0.25f) : Color.white;
        }

        // ── Name ──────────────────────────────────────────────────────────────
        if (itemNameText != null)
        {
            itemNameText.text  = slot.Item?.DisplayName ?? string.Empty;
            itemNameText.color = isEmpty ? new Color(1f, 1f, 1f, 0.40f) : Color.white;
        }

        // ── Quantity ──────────────────────────────────────────────────────────
        if (quantityText != null)
        {
            quantityText.text  = isEmpty ? "0" : slot.Quantity.ToString();
            quantityText.color = isEmpty
                ? new Color(1f, 0.30f, 0.30f, 1f)   // red when empty
                : new Color(1f, 0.90f, 0.30f, 1f);   // yellow when available
        }

        // ── Background ────────────────────────────────────────────────────────
        if (backgroundImage != null)
        {
            backgroundImage.color = isEmpty
                ? EmptyColor
                : (isSelected ? SelectedColor : NormalColor);
        }

        // ── Selection highlight ───────────────────────────────────────────────
        if (selectionHighlight != null)
            selectionHighlight.enabled = isSelected && !isEmpty;
    }

    /// <summary>
    /// Sets the selected visual state of this slot.
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (backgroundImage != null)
            backgroundImage.color = selected ? SelectedColor : (slot != null && slot.IsEmpty ? EmptyColor : NormalColor);

        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;
    }

    // ── Pointer events ────────────────────────────────────────────────────────

    public void OnPointerClick(PointerEventData eventData)
    {
        if (slot == null || slot.IsEmpty) return;
        onClickCallback?.Invoke(slot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected && backgroundImage != null && slot != null && !slot.IsEmpty)
            backgroundImage.color = HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && backgroundImage != null && slot != null)
            backgroundImage.color = slot.IsEmpty ? EmptyColor : NormalColor;
    }
}
