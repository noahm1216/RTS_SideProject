using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region COSMETIC SYSTEM

/// <summary>
/// Handles runtime cosmetic control and data syncing.
/// </summary>
public class UnitCosmetics : MonoBehaviour
{
    public bool CanDragToScale { get; private set; }
    public ScaleOnDrag[] ScaleComponents { get; private set; }
    public Transform[] BodyParts { get; private set; }
    private Vector3[] _factoryScale;

    private void Awake()
    {
        GetAllBodyParts();
    }

    public void GetAllBodyParts()
    {
        BodyParts = GetComponentsInChildren<Transform>();
        ScaleComponents = new ScaleOnDrag[BodyParts.Length];
        _factoryScale = new Vector3[BodyParts.Length];

        for (int i = 0; i < BodyParts.Length; i++)
        {
            ScaleComponents[i] = BodyParts[i].GetComponent<ScaleOnDrag>();
            _factoryScale[i] = BodyParts[i].localScale;
        }
    }

    public void ChangeCanDragToScale(bool enable)
    {
        CanDragToScale = enable;
        foreach (var scale in ScaleComponents)
        {
            if (scale != null)
                scale.enabled = enable;
        }
    }

    public void ApplyCosmeticData(UnitCosmeticData data)
    {
        if (BodyParts == null || BodyParts.Length == 0)
            GetAllBodyParts();

        if (data == null)
        {
            Debug.LogWarning("Tried to apply null cosmetic data.");
            return;
        }

        // Apply color schemes
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            var mats = renderer.materials;
            foreach (var mat in mats)
            {
                mat.color = data.colorScheme1;
            }
        }

        // Apply scale data
        for (int i = 0; i < BodyParts.Length && i < data.bodyPartsScale.Length; i++)
            BodyParts[i].localScale = data.bodyPartsScale[i];
    }

    public UnitCosmeticData ExtractCosmeticData()
    {
        if (BodyParts == null || BodyParts.Length == 0)
            GetAllBodyParts();

        UnitCosmeticData data = new UnitCosmeticData
        {
            bodyPartsScale = new Vector3[BodyParts.Length]
        };

        for (int i = 0; i < BodyParts.Length; i++)
            data.bodyPartsScale[i] = BodyParts[i].localScale;

        return data;
    }

    public void ResetToFactoryScale()
    {
        for (int i = 0; i < BodyParts.Length; i++)
            BodyParts[i].localScale = _factoryScale[i];
    }
}

#endregion