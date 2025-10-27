using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class UnitCosmetics : MonoBehaviour
{
    [Header("Shader Property Names")]
    [Tooltip("Name of the outline color property in your shader.")]
    [SerializeField] private string outlineColorProperty = "_OutlineColor";

    [Tooltip("Name of the outline width property in your shader.")]
    [SerializeField] private string outlineSizeProperty = "_FadeExterior";

    [Tooltip("Shader color remap properties for skin, shirt, and pants.")]
    [SerializeField] private string redChannelProperty = "_Color_Replace_Red";
    [SerializeField] private string greenChannelProperty = "_Color_Replace_Blue";
    [SerializeField] private string blueChannelProperty = "_Color_Replace_Green";

    [HideInInspector] public Transform[] BodyParts;
    private Vector3[] _factoryScale;

    private void OnEnable()
    {
        if (!Application.isPlaying)
            RefreshBodyParts();
    }

    public void RefreshBodyParts()
    {
        BodyParts = GetComponentsInChildren<Transform>();
        _factoryScale = new Vector3[BodyParts.Length];
        for (int i = 0; i < BodyParts.Length; i++)
            _factoryScale[i] = BodyParts[i].localScale;
    }

    public void ApplyCosmeticData(UnitCosmeticData data)
    {
        if (data == null)
        {
            Debug.LogWarning("No cosmetic data assigned.");
            return;
        }

        if (BodyParts == null || BodyParts.Length == 0)
            RefreshBodyParts();

        // --- Apply scale ---
        if (data.bodyPartsScale != null)
        {
            for (int i = 0; i < BodyParts.Length && i < data.bodyPartsScale.Length; i++)
                BodyParts[i].localScale = data.bodyPartsScale[i];
        }

        // --- Apply shader values ---
        ApplyShaderProperties(data);
    }

    private void ApplyShaderProperties(UnitCosmeticData data)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.sharedMaterials)
            {
                if (mat.HasProperty(outlineColorProperty))
                    mat.SetColor(outlineColorProperty, data.outlineColor);

                if (mat.HasProperty(outlineSizeProperty))
                    mat.SetFloat(outlineSizeProperty, data.outlineSize);

                if (mat.HasProperty(redChannelProperty))
                    mat.SetColor(redChannelProperty, data.colorSkin);

                if (mat.HasProperty(greenChannelProperty))
                    mat.SetColor(greenChannelProperty, data.colorMain);

                if (mat.HasProperty(blueChannelProperty))
                    mat.SetColor(blueChannelProperty, data.colorSecondary);
            }
        }
    }

    public UnitCosmeticData ExtractCosmeticData()
    {
        if (BodyParts == null || BodyParts.Length == 0)
            RefreshBodyParts();

        UnitCosmeticData data = new UnitCosmeticData();
        data.bodyPartsScale = new Vector3[BodyParts.Length];

        for (int i = 0; i < BodyParts.Length; i++)
            data.bodyPartsScale[i] = BodyParts[i].localScale;

        // If you want to capture material values too (for editing existing presets)
        var r = GetComponentInChildren<Renderer>();
        if (r && r.sharedMaterial)
        {
            var mat = r.sharedMaterial;
            if (mat.HasProperty(outlineColorProperty)) data.outlineColor = mat.GetColor(outlineColorProperty);
            if (mat.HasProperty(outlineSizeProperty)) data.outlineSize = mat.GetFloat(outlineSizeProperty);
            if (mat.HasProperty(redChannelProperty)) data.colorSkin = mat.GetColor(redChannelProperty);
            if (mat.HasProperty(greenChannelProperty)) data.colorMain = mat.GetColor(greenChannelProperty);
            if (mat.HasProperty(blueChannelProperty)) data.colorSecondary = mat.GetColor(blueChannelProperty);
        }

        return data;
    }

    public void ResetToFactoryScale()
    {
        if (BodyParts == null || BodyParts.Length == 0)
            RefreshBodyParts();

        for (int i = 0; i < BodyParts.Length; i++)
            BodyParts[i].localScale = _factoryScale[i];
    }
}
