using UnityEngine;

public class CosmeticTestPanel : MonoBehaviour
{
    [Header("Unit Setup")]
    public ManagerUnits unitManager;
    public UnitData unitTemplate;

    [Header("Spawn Settings")]
    public Vector3 spawnPosition = new Vector3(0, 0, 0);
    private Unit spawnedUnit;

    private void Start()
    {
        if (unitManager == null)
        {
            unitManager = FindObjectOfType<ManagerUnits>();
        }

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
        spawnedUnit.NickName = "TestUnit";
    }

    private void OnGUI()
    {
        if (spawnedUnit == null)
        {
            GUILayout.Label("No unit spawned.");
            return;
        }

        GUILayout.BeginArea(new Rect(20, 20, 250, 400), "Unit Cosmetic Controls", GUI.skin.window);
        GUILayout.Label($"Unit: {spawnedUnit.Race}");

        if (GUILayout.Button("Randomize Colors"))
        {
            RandomizeColors();
        }

        if (GUILayout.Button("Save Preset A"))
        {
            UnitCosmeticSaveSystem.SaveCosmetics(spawnedUnit, "PresetA");
        }

        if (GUILayout.Button("Load Preset A"))
        {
            UnitCosmeticSaveSystem.LoadCosmetics(spawnedUnit, "PresetA");
        }

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

        newData.colorScheme1 = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);
        newData.colorScheme2 = Random.ColorHSV(0f, 1f, 0.3f, 1f, 0.8f, 1f);
        newData.colorSkin = Random.ColorHSV(0f, 0.2f, 0.4f, 0.8f, 0.6f, 1f);

        spawnedUnit.ApplyCosmeticData(newData);
    }
}
