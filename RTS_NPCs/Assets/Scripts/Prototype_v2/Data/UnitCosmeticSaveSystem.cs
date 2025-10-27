using UnityEngine;
using System.IO;

public static class UnitCosmeticSaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "CosmeticSaves");

    /// <summary>
    /// Saves the current cosmetic data of a unit to a JSON file.
    /// </summary>
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

        // Ensure directory exists
        Directory.CreateDirectory(SavePath);

        // File name includes race for easy sorting
        string fileName = $"{unit.Race}_{presetName}.json";
        string filePath = Path.Combine(SavePath, fileName);

        // Serialize to JSON
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"[UnitCosmeticSaveSystem] Saved cosmetic preset '{presetName}' for {unit.Race} at:\n{filePath}");
    }

    /// <summary>
    /// Loads cosmetic data from JSON and applies it to the given unit.
    /// </summary>
    public static void LoadCosmetics(Unit unit, string presetName)
    {
        if (unit == null)
        {
            Debug.LogWarning("Cannot load cosmetics: Unit is null.");
            return;
        }

        string filePath = Path.Combine(SavePath, $"{unit.Race}_{presetName}.json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"No saved preset found at: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        UnitCosmeticData loadedData = JsonUtility.FromJson<UnitCosmeticData>(json);

        if (loadedData == null)
        {
            Debug.LogError($"Failed to parse cosmetic preset JSON at {filePath}");
            return;
        }

        unit.ApplyCosmeticData(loadedData);
        Debug.Log($"[UnitCosmeticSaveSystem] Loaded cosmetic preset '{presetName}' for {unit.Race}");
    }

    /// <summary>
    /// Deletes a saved cosmetic preset file for the given race and name.
    /// </summary>
    public static void DeletePreset(string presetName, UnitData.UnitRace race)
    {
        string filePath = Path.Combine(SavePath, $"{race}_{presetName}.json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[UnitCosmeticSaveSystem] Deleted cosmetic preset '{presetName}' for race: {race}");
        }
        else
        {
            Debug.LogWarning($"[UnitCosmeticSaveSystem] No preset found to delete at: {filePath}");
        }
    }

    /// <summary>
    /// Checks if a specific preset exists.
    /// </summary>
    public static bool PresetExists(UnitData.UnitRace race, string presetName)
    {
        string filePath = Path.Combine(SavePath, $"{race}_{presetName}.json");
        return File.Exists(filePath);
    }

    /// <summary>
    /// Lists all available cosmetic preset files.
    /// </summary>
    public static string[] GetAllPresets()
    {
        if (!Directory.Exists(SavePath))
            return new string[0];

        string[] files = Directory.GetFiles(SavePath, "*.json");
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileNameWithoutExtension(files[i]);

        return files;
    }
}
