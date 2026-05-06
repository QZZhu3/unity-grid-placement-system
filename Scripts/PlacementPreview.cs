using UnityEngine;

/// <summary>
/// Manages the visual preview of a placeable item before it's placed.
/// Shows green when placement is valid, red when invalid.
/// Compatible with both URP and Built-in Render Pipeline.
/// </summary>
public class PlacementPreview : MonoBehaviour
{
    private Material validMaterial;
    private Material invalidMaterial;
    private Renderer[] renderers;
    private bool isValid = true;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        CreatePreviewMaterials();
        ApplyMaterial(validMaterial);
    }

    /// <summary>
    /// Creates semi-transparent materials for valid (green) and invalid (red) states.
    /// Automatically detects URP or Built-in Render Pipeline.
    /// </summary>
    private void CreatePreviewMaterials()
    {
        Shader shader = FindCompatibleTransparentShader();

        validMaterial = new Material(shader);
        SetTransparentColor(validMaterial, new Color(0f, 1f, 0f, 0.5f));

        invalidMaterial = new Material(shader);
        SetTransparentColor(invalidMaterial, new Color(1f, 0f, 0f, 0.5f));
    }

    /// <summary>
    /// Finds a compatible transparent shader — prefers URP Lit, falls back to Built-in Standard.
    /// </summary>
    private Shader FindCompatibleTransparentShader()
    {
        // Try URP shaders first
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null) return urpLit;

        Shader urpSimple = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (urpSimple != null) return urpSimple;

        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (urpUnlit != null) return urpUnlit;

        // Fallback to Built-in Standard
        return Shader.Find("Standard");
    }

    /// <summary>
    /// Sets the color and enables transparency on a material.
    /// Handles both URP and Built-in pipeline transparency settings.
    /// </summary>
    private void SetTransparentColor(Material mat, Color color)
    {
        mat.color = color;

        // URP transparency setup
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f); // 1 = Transparent in URP
            mat.SetFloat("_Blend", 0f);
            mat.SetFloat("_AlphaClip", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
        }
        else
        {
            // Built-in Standard transparency setup
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }

    /// <summary>
    /// Applies a material to all renderers on the preview object.
    /// </summary>
    private void ApplyMaterial(Material mat)
    {
        foreach (Renderer r in renderers)
        {
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.materials = mats;
        }
    }

    /// <summary>
    /// Sets the validity state and updates the preview color.
    /// </summary>
    public void SetValidity(bool valid)
    {
        if (isValid == valid) return;
        isValid = valid;
        ApplyMaterial(isValid ? validMaterial : invalidMaterial);
    }

    public bool IsValid => isValid;

    private void OnDestroy()
    {
        if (validMaterial != null) Destroy(validMaterial);
        if (invalidMaterial != null) Destroy(invalidMaterial);
    }
}
