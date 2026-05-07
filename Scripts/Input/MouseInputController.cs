using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Concrete implementation of IInputController for mouse input.
/// Uses the New Input System (UnityEngine.InputSystem).
/// Attach to the same GameObject as PlacementManager.
/// </summary>
public class MouseInputController : MonoBehaviour, IInputController
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float raycastDistance = 1000f;

    private Vector3 cachedWorldPosition = Vector3.zero;
    private bool worldPositionValid = false;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateWorldPosition();
    }

    /// <summary>
    /// Casts a ray from the camera through the cursor each frame and caches the world hit point.
    /// </summary>
    private void UpdateWorldPosition()
    {
        Vector2 screen = ScreenPosition;
        Ray ray = mainCamera.ScreenPointToRay(screen);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayerMask))
        {
            cachedWorldPosition = hit.point;
            worldPositionValid = true;
        }
        else
        {
            worldPositionValid = false;
        }
    }

    // ── IInputController implementation ──────────────────────────────────────

    /// <inheritdoc/>
    public Vector3 WorldPosition => worldPositionValid ? cachedWorldPosition : Vector3.zero;

    /// <inheritdoc/>
    public bool ConfirmPressed =>
        Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    /// <inheritdoc/>
    public bool CancelPressed =>
        (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) ||
        (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);

    /// <inheritdoc/>
    public bool RotatePressed =>
        Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;

    /// <inheritdoc/>
    public bool PickUpPressed =>
        Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

    /// <inheritdoc/>
    public Vector2 ScreenPosition =>
        Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

    /// <summary>
    /// Exposes whether the last raycast hit the ground, for external checks.
    /// </summary>
    public bool IsOverGround => worldPositionValid;

    /// <summary>
    /// Exposes the camera and ground mask so PlacementManager can do pick-up raycasts.
    /// </summary>
    public Camera MainCamera => mainCamera;
    public LayerMask GroundLayerMask => groundLayerMask;
    public float RaycastDistance => raycastDistance;
}
