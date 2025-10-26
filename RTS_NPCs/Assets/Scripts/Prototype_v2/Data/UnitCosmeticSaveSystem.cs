using UnityEngine;
using System.IO;

public static class UnitCosmeticSaveSystem
{
    private static string SavePath => Application.persistentDataPath + "/CosmeticSaves/";

    public static void SaveCosmetics(Unit unit, string presetName)
    {
        if (unit == null)
        {
            Debug.LogWarning("Cannot save cosmetics: Unit is null.");
            return;
        }

        var data = unit.GetCurrentCosmeticData();
        if (data == null)
        {
            Debug.LogWarning("Cannot save cosmetics: UnitCosmeticData is null.");
            return;
        }

        string json = JsonUtility.ToJson(data, true);
        Directory.CreateDirectory(SavePath);

        string filePath = Path.Combine(SavePath, $"{presetName}_{unit.Race}.json");
        File.WriteAllText(filePath, json);

        Debug.Log($"Saved cosmetic preset '{presetName}' for {unit.Race} at: {filePath}");
    }

    public static void LoadCosmetics(Unit unit, string presetName)
    {
        if (unit == null)
        {
            Debug.LogWarning("Cannot load cosmetics: Unit is null.");
            return;
        }

        string filePath = Path.Combine(SavePath, $"{presetName}_{unit.Race}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"No saved preset found at: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        var data = JsonUtility.FromJson<UnitCosmeticData>(json);
        unit.ApplyCosmeticData(data);

        Debug.Log($"Loaded cosmetic preset '{presetName}' for {unit.Race}");
    }

    public static void DeletePreset(string presetName, UnitData.UnitRace race)
    {
        string filePath = Path.Combine(SavePath, $"{presetName}_{race}.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Deleted cosmetic preset: {presetName} for race: {race}");
        }
        else
        {
            Debug.LogWarning($"No preset found to delete: {filePath}");
        }
    }
}
