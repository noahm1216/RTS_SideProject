using System.Collections.Generic;
using UnityEngine;

#region VISIBLE RUNTIME FIELDS
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Unit))]
public class UnitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Unit unit = (Unit)target;
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current HP", unit.CurrentHP.ToString());
            EditorGUILayout.LabelField("Current Action", unit.CurrentAction.ToString());
        }
    }
}
#endif
#endregion


#region DATA DEFINITIONS

[CreateAssetMenu(fileName = "New Unit Data", menuName = "ScriptableObjects/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    public enum UnitRace { Human, Elf, Nature }
    public enum ActionTaking { Idle, Walking, Gathering, Building, Attacking, Relaxing }

    [Header("Identity")]
    [Tooltip("Which race this unit belongs to.")]
    public UnitRace race;

    [Tooltip("Icon used in UI or unit selection panels.")]
    public Sprite icon;

    [Header("Prefab Reference")]
    [Tooltip("The 3D prefab model used when spawning this unit.")]
    public GameObject basePrefab;

    [Header("Base Stats")]
    [Tooltip("Total health points of the unit.")]
    [Min(1)]
    public int hp = 100;

    [Tooltip("Movement speed of the unit in world space.")]
    [Range(0.1f, 20f)]
    public float speedMove = 5f;

    [Tooltip("Speed at which the unit performs actions like attacking or building.")]
    [Range(0.1f, 10f)]
    public float speedAction = 1f;

    [Header("Default Appearance")]
    [Tooltip("Default color and scale cosmetic data applied to this unit when spawned.")]
    public UnitCosmeticData defaultCosmetics;
}

#endregion



#region RUNTIME UNIT



public class Unit : MonoBehaviour
{
    //[Header("Runtime Data")]
    [Tooltip("The UnitData this unit is based on.")]
    public UnitData Data { get; private set; }

    [Tooltip("The race this unit belongs to.")]
    public UnitData.UnitRace Race { get; private set; }

    [Tooltip("Unit's runtime cosmetic handler.")]
    public UnitCosmetics Cosmetics { get; private set; }

    //[Header("Current State")]
    public int CurrentHP { get; private set; }
    public float SpeedMove { get; private set; }
    public float SpeedAction { get; private set; }

    //[Space]
    public bool IsSelected { get; set; }
    public string NickName { get; set; } = "";
    public UnitData.ActionTaking CurrentAction { get; private set; }

    [Tooltip("All tracked experiences from performed actions.")]
    public List<UnitExperience> Experience { get; private set; } = new List<UnitExperience>();

    // Initialize from ScriptableObject data
    public void InitializeFromData(UnitData data)
    {
        Data = data;
        Race = data.race;
        CurrentHP = data.hp;
        SpeedMove = data.speedMove;
        SpeedAction = data.speedAction;

        if (TryGetComponent(out UnitCosmetics cosmetics)) Cosmetics = cosmetics;
        else Debug.LogWarning($"Unit '{gameObject.name}' is missing a UnitCosmetics component.");

        // Initialize cosmetics based on data’s default or saved preset
        Cosmetics.ApplyCosmeticData(data.defaultCosmetics);
    }

    // Apply saved or modified cosmetics
    public void ApplyCosmeticData(UnitCosmeticData newData)
    {
        if (Cosmetics != null)
            Cosmetics.ApplyCosmeticData(newData);
    }

    // Extract the current cosmetic state (for saving)
    public UnitCosmeticData GetCurrentCosmeticData()
    {
        return Cosmetics != null ? Cosmetics.ExtractCosmeticData() : null;
    }

    // Example of runtime interaction
    public void SetAction(UnitData.ActionTaking action)
    {
        CurrentAction = action;
        RecordExperience(action);
    }

    private void RecordExperience(UnitData.ActionTaking action)
    {
        var exp = Experience.Find(x => x.action == action);
        if (exp == null)
            Experience.Add(new UnitExperience(1, action));
        else
            exp.timesActed++;
    }
}

#endregion




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




#region SUPPORTING DATA

[System.Serializable]
public class UnitExperience
{
    public UnitData.ActionTaking action;
    public int timesActed = 0;

    public UnitExperience(int _timesActed, UnitData.ActionTaking _action)
    {
        timesActed = _timesActed;
        action = _action;
    }
}


[System.Serializable]
public class UnitCosmeticData
{
    [Header("Colors")]
    [Tooltip("Base skin tone for this unit.")]
    public Color colorSkin = Color.white;

    [Tooltip("Primary color for clothing or armor.")]
    public Color colorScheme1 = Color.gray;

    [Tooltip("Secondary color for accents or patterns.")]
    public Color colorScheme2 = Color.black;

    [Header("Body Scale Data")]
    [Tooltip("Stores the local scale of each body part for customization.")]
    public Vector3[] bodyPartsScale;
}

#endregion
