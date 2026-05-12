using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Handles the soft background blur and darkening when the journal is pinned.
/// Creates a cozy, dreamy focus state without harsh interruptions.
/// </summary>
public class JournalBlurController : MonoBehaviour
{
    [Header("UI Overlay")]
    [Tooltip("A full-screen UI Image behind the journal used to darken the screen.")]
    [SerializeField] private Image darkenOverlay;
    [SerializeField] private float targetDarkenAlpha = 0.4f;

    [Header("Volume Blur (Optional)")]
    [Tooltip("Global Volume containing a Depth of Field override.")]
    [SerializeField] private Volume blurVolume;
    [SerializeField] private float targetBlurWeight = 1f;

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.InOut(0f, 0f, 1f, 1f);

    private Coroutine activeTransition;
    private bool isBlurActive = false;

    private void Start()
    {
        if (darkenOverlay != null)
        {
            Color c = darkenOverlay.color;
            c.a = 0f;
            darkenOverlay.color = c;
            darkenOverlay.raycastTarget = false;
        }

        if (blurVolume != null)
        {
            blurVolume.weight = 0f;
        }
    }

    public void SetBlurActive(bool active, bool immediate = false)
    {
        if (isBlurActive == active) return;
        isBlurActive = active;

        float endAlpha = active ? targetDarkenAlpha : 0f;
        float endWeight = active ? targetBlurWeight : 0f;

        if (immediate)
        {
            if (activeTransition != null) StopCoroutine(activeTransition);

            if (darkenOverlay != null)
            {
                Color c = darkenOverlay.color;
                c.a = endAlpha;
                darkenOverlay.color = c;
                darkenOverlay.raycastTarget = active;
            }

            if (blurVolume != null) blurVolume.weight = endWeight;
            return;
        }

        if (activeTransition != null) StopCoroutine(activeTransition);
        activeTransition = StartCoroutine(AnimateBlur(endAlpha, endWeight, active));
    }

    private IEnumerator AnimateBlur(float targetAlpha, float targetWeight, bool active)
    {
        float startAlpha = darkenOverlay != null ? darkenOverlay.color.a : 0f;
        float startWeight = blurVolume != null ? blurVolume.weight : 0f;
        float time = 0f;

        if (active && darkenOverlay != null) darkenOverlay.raycastTarget = true;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = easeCurve.Evaluate(time / transitionDuration);

            if (darkenOverlay != null)
            {
                Color c = darkenOverlay.color;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                darkenOverlay.color = c;
            }

            if (blurVolume != null)
            {
                blurVolume.weight = Mathf.Lerp(startWeight, targetWeight, t);
            }

            yield return null;
        }

        if (darkenOverlay != null)
        {
            Color c = darkenOverlay.color;
            c.a = targetAlpha;
            darkenOverlay.color = c;
            if (!active) darkenOverlay.raycastTarget = false;
        }

        if (blurVolume != null) blurVolume.weight = targetWeight;
    }
}
