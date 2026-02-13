using UnityEngine;

public partial class TreeGenerator
{
    private void ConfigureLeafMaterialTransparency(GameObject leafObject)
    {
        MeshRenderer leafRenderer = leafObject.GetComponent<MeshRenderer>();
        if (leafRenderer == null || leafRenderer.sharedMaterial == null)
            return;

        // Create a material instance (avoid leaks in edit mode)
        Material leafMatInstance = new Material(leafRenderer.sharedMaterial);
        leafRenderer.sharedMaterial = leafMatInstance;

        // Set transparency via material properties
        if (leafMatInstance.HasProperty("_Color"))
        {
            Color matColor = leafMatInstance.GetColor("_Color");
            matColor.a = leafTransparency;
            leafMatInstance.SetColor("_Color", matColor);
        }

        // Enable transparent rendering mode if needed
        if (leafTransparency < 1f)
        {
            SetupTransparentRenderMode(leafMatInstance);
        }
    }

    private void SetupTransparentRenderMode(Material material)
    {
        material.SetFloat("_Mode", TransparentModeValue);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = TransparentRenderQueue;
    }
}
