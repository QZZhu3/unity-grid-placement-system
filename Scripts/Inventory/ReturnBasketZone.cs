using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A UI zone that acts as a return basket.
/// Listens to pointer events to detect when the mouse is hovering over it.
/// If the player clicks (Confirm) while hovering over this zone during a drag,
/// the PlacementManager should intercept it and cancel the placement (returning to inventory).
/// </summary>
public class ReturnBasketZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject visualContainer;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Visual Feedback")]
    [SerializeField] private float normalAlpha = 0.6f;
    [SerializeField] private float hoverAlpha = 1.0f;
    [SerializeField] private float hoverScale = 1.1f;

    private PlacementManager placementManager;

    private void Start()
    {
        placementManager = FindAnyObjectByType<PlacementManager>();
        if (placementManager != null)
        {
            placementManager.OnDragStarted += HandleDragStarted;
            placementManager.OnDragEnded += HandleDragEnded;
        }

        SetVisible(false);
    }

    private void HandleDragStarted(DraggableItem item)
    {
        SetVisible(true);
        SetHoverState(false); // reset highlight
    }

    private void HandleDragEnded(DraggableItem item)
    {
        SetVisible(false);
        isHovering = false;
    }

    private void SetVisible(bool visible)
    {
        if (visualContainer != null)
            visualContainer.SetActive(visible);
    }

    private void SetHoverState(bool hovering)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = hovering ? hoverAlpha : normalAlpha;

        if (visualContainer != null)
            visualContainer.transform.localScale = hovering ? Vector3.one * hoverScale : Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (placementManager != null && placementManager.IsDragging)
        {
            SetHoverState(true);
            placementManager.SetHoveringReturnBasket(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (placementManager != null && placementManager.IsDragging)
        {
            SetHoverState(false);
            placementManager.SetHoveringReturnBasket(false);
        }
    }

    private void OnDestroy()
    {
        if (placementManager != null)
        {
            placementManager.OnDragStarted -= HandleDragStarted;
            placementManager.OnDragEnded -= HandleDragEnded;
        }
    }
}
