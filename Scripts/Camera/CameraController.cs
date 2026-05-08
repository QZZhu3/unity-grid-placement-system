using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages a cozy 2.5D camera that smoothly transitions between preset angles and supports zooming.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetFocus; // Usually the center of the grid
    [SerializeField] private Camera cam;

    [Header("Presets")]
    [SerializeField] private CameraPreset[] presets;
    [SerializeField] private int currentPresetIndex = 0;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Zoom Settings")]
    [SerializeField] private float minZoomMultiplier = 0.5f;
    [SerializeField] private float maxZoomMultiplier = 1.5f;
    [SerializeField] private float zoomSpeed = 0.1f;
    
    private float currentZoomMultiplier = 1f;
    private float targetZoomMultiplier = 1f;

    private Vector3 currentPosOffset;
    private Quaternion currentRotation;
    private float currentFov;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (presets == null || presets.Length == 0)
        {
            Debug.LogWarning("[CameraController] No presets defined!");
            return;
        }

        // Initialize immediately to the first preset
        ApplyPreset(presets[currentPresetIndex], true);
    }

    private void Update()
    {
        HandleInput();
        UpdateTransform();
    }

    private void HandleInput()
    {
        // Keyboard rotation toggle
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            PreviousPreset();
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            NextPreset();

        // Mouse scroll zoom
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0)
            {
                targetZoomMultiplier -= Mathf.Sign(scroll) * zoomSpeed;
                targetZoomMultiplier = Mathf.Clamp(targetZoomMultiplier, minZoomMultiplier, maxZoomMultiplier);
            }
        }
    }

    private void UpdateTransform()
    {
        if (presets == null || presets.Length == 0 || targetFocus == null) return;

        CameraPreset targetPreset = presets[currentPresetIndex];
        
        // Smoothly interpolate zoom
        currentZoomMultiplier = Mathf.Lerp(currentZoomMultiplier, targetZoomMultiplier, Time.deltaTime * transitionSpeed);

        // Smoothly interpolate properties
        currentPosOffset = Vector3.Lerp(currentPosOffset, targetPreset.positionOffset * currentZoomMultiplier, Time.deltaTime * transitionSpeed);
        currentRotation = Quaternion.Slerp(currentRotation, Quaternion.Euler(targetPreset.rotation), Time.deltaTime * transitionSpeed);
        currentFov = Mathf.Lerp(currentFov, targetPreset.fieldOfView, Time.deltaTime * transitionSpeed);

        // Apply to camera transform
        transform.position = targetFocus.position + currentPosOffset;
        transform.rotation = currentRotation;
        cam.fieldOfView = currentFov;
    }

    public void NextPreset()
    {
        if (presets == null || presets.Length == 0) return;
        currentPresetIndex = (currentPresetIndex + 1) % presets.Length;
    }

    public void PreviousPreset()
    {
        if (presets == null || presets.Length == 0) return;
        currentPresetIndex--;
        if (currentPresetIndex < 0) currentPresetIndex = presets.Length - 1;
    }

    public void SetPreset(int index)
    {
        if (presets == null || index < 0 || index >= presets.Length) return;
        currentPresetIndex = index;
    }

    private void ApplyPreset(CameraPreset preset, bool instant = false)
    {
        if (instant)
        {
            currentPosOffset = preset.positionOffset * targetZoomMultiplier;
            currentRotation = Quaternion.Euler(preset.rotation);
            currentFov = preset.fieldOfView;
            
            if (targetFocus != null)
            {
                transform.position = targetFocus.position + currentPosOffset;
                transform.rotation = currentRotation;
                cam.fieldOfView = currentFov;
            }
        }
    }
}
