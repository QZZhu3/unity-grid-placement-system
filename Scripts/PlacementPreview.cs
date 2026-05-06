using UnityEngine;

/// <summary>
/// Manages the visual preview of a placeable item before it's placed.
/// Shows green when placement is valid, red when invalid.
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
    }

    /// <summary>
    /// Creates materials for valid and invalid placement states.
    /// </summary>
    private void CreatePreviewMaterials()
    {
        validMaterial = new Material(Shader.Find("Standard"));
        validMaterial.color = new Color(0, 1, 0, 0.5f);
        validMaterial.SetFloat("_Mode", 3);
        validMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        validMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        validMaterial.SetInt("_ZWrite", 0);
        validMaterial.renderQueue = 3000;

        invalidMaterial = new Material(Shader.Find("Standard"));
        invalidMaterial.color = new Color(1, 0, 0, 0.5f);
        invalidMaterial.SetFloat("_Mode", 3);
        invalidMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        invalidMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        invalidMaterial.SetInt("_ZWrite", 0);
        invalidMaterial.renderQueue = 3000;
    }

    /// <summary>
    /// Sets the validity state of the preview.
    /// </summary>
    public void SetValidity(bool valid)
    {
        if (isValid == valid)
            return;

        isValid = valid;
        Material materialToUse = isValid ? validMaterial : invalidMaterial;

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialToUse;
            }
            renderer.materials = materials;
        }
    }

    /// <summary>
    /// Gets the current validity state.
    /// </summary>
    public bool IsValid => isValid;

    /// <summary>
    /// Cleans up materials when destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (validMaterial != null)
            Destroy(validMaterial);
        if (invalidMaterial != null)
            Destroy(invalidMaterial);
    }
}
