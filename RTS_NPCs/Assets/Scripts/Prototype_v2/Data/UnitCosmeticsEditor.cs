using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(UnitCosmetics))]
public class UnitCosmeticsEditor : Editor
{
    private string presetName = "MyPreset";
    private static string EditorPresetPath => Path.Combine(Application.persistentDataPath, "ModPresets/");    //private const string EditorPresetPath = "Assets/ModPresets/";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnitCosmetics cosmetics = (UnitCosmetics)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Cosmetic Preset Tools", EditorStyles.boldLabel);

        // Preset Name Input
        presetName = EditorGUILayout.TextField("Preset Name", presetName);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Preset (JSON)"))
        {
            SavePreset(cosmetics);
        }

        if (GUILayout.Button("Load Preset (JSON)"))
        {
            LoadPreset(cosmetics);
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reset to Factory"))
        {
            cosmetics.ResetToFactoryScale();
        }
    }

    private void SavePreset(UnitCosmetics cosmetics)
    {
        if (cosmetics == null)
        {
            Debug.LogWarning("No UnitCosmetics target found to save.");
            return;
        }

        UnitCosmeticData data = cosmetics.ExtractCosmeticData();
        if (data == null)
        {
            Debug.LogWarning("Could not extract cosmetic data.");
            return;
        }

        Directory.CreateDirectory(EditorPresetPath);
        string filePath = Path.Combine(EditorPresetPath, $"{presetName}.json");

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log($"[UnitCosmeticsEditor] Saved cosmetic preset to {filePath}");
        AssetDatabase.Refresh();
    }

    private void LoadPreset(UnitCosmetics cosmetics)
    {
        string filePath = EditorUtility.OpenFilePanel("Select Cosmetic Preset", EditorPresetPath, "json");
        if (string.IsNullOrEmpty(filePath))
            return;

        string json = File.ReadAllText(filePath);
        UnitCosmeticData data = JsonUtility.FromJson<UnitCosmeticData>(json);

        if (data == null)
        {
            Debug.LogError($"[UnitCosmeticsEditor] Failed to load preset from: {filePath}");
            return;
        }

        cosmetics.ApplyCosmeticData(data);
        Debug.Log($"[UnitCosmeticsEditor] Loaded preset '{Path.GetFileNameWithoutExtension(filePath)}'");
    }
}
