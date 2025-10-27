using UnityEngine;

[System.Serializable]
public class UnitCosmeticData
{
    [Header("Basic Color Scheme")]
    public Color colorSkin = Color.white;   // Red channel
    public Color colorMain = Color.gray;   // Green channel
    public Color colorSecondary = Color.black;  // Blue channel

    [Header("Outline Settings")]
    [Range(0.6f, 1.0f)] public float outlineSize = 0.75f;
    public Color outlineColor = Color.black;

    [Header("Body Scale Data")]
    public Vector3[] bodyPartsScale;
}
