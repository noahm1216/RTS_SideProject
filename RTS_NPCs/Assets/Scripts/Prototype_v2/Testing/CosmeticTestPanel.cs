using UnityEngine;

public class CosmeticTestPanel : MonoBehaviour
{
    [Header("Unit Setup")]
    public ManagerUnits unitManager;
    public UnitData unitTemplate;

    [Header("Spawn Settings")]
    public Vector3 spawnPosition = Vector3.zero;
    private Unit spawnedUnit;

    private void Start()
    {
        if (unitManager == null)
            unitManager = ManagerUnits.Instance;

        SpawnUnit();
    }

    private void SpawnUnit()
    {
        if (unitTemplate == null)
        {
            Debug.LogError("Missing UnitData template reference.");
            return;
        }

        spawnedUnit = unitManager.SpawnUnit(unitTemplate, spawnPosition);
        if (spawnedUnit == null)
        {
            Debug.LogError("ManagerUnits failed to spawn unit.");
            return;
        }

        spawnedUnit.NickName = "TestUnit";
    }

    private void OnGUI()
    {
        if (spawnedUnit == null)
        {
            GUILayout.Label("No unit spawned.");
            return;
        }

        GUILayout.BeginArea(new Rect(20, 20, 260, 480), "Unit Cosmetic Controls", GUI.skin.window);
        GUILayout.Label($"Unit: {spawnedUnit.Race}");
        GUILayout.Space(10);

        if (GUILayout.Button("Randomize Colors"))
        {
            RandomizeColors();
        }

        if (GUILayout.Button("Randomize Outline"))
        {
            RandomizeOutline();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Save Preset A"))
        {
            UnitCosmeticSaveSystem.SaveCosmetics(spawnedUnit, "PresetA");
        }

        if (GUILayout.Button("Load Preset A"))
        {
            UnitCosmeticSaveSystem.LoadCosmetics(spawnedUnit, "PresetA");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Reset to Default"))
        {
            spawnedUnit.ApplyCosmeticData(unitTemplate.defaultCosmetics);
        }

        if (GUILayout.Button("Delete Preset A"))
        {
            UnitCosmeticSaveSystem.DeletePreset("PresetA", spawnedUnit.Race);
        }

        GUILayout.EndArea();
    }

    private void RandomizeColors()
    {
        UnitCosmeticData newData = spawnedUnit.GetCurrentCosmeticData();

        newData.colorSkin = Random.ColorHSV(0f, 1f, 0.3f, 0.8f, 0.7f, 1f);     // skin tone
        newData.colorMain = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);     // shirt
        newData.colorSecondary = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.3f, 1f);     // pants

        spawnedUnit.ApplyCosmeticData(newData);
    }

    private void RandomizeOutline()
    {
        UnitCosmeticData newData = spawnedUnit.GetCurrentCosmeticData();

        newData.outlineColor = Random.ColorHSV(0f, 1f, 0.4f, 1f, 0.5f, 1f);
        newData.outlineSize = Random.Range(0.6f, 1.0f);

        spawnedUnit.ApplyCosmeticData(newData);
    }
}
