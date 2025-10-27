using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CosmeticRuntimePanel : MonoBehaviour
{
    [Header("References")]
    public ManagerUnits unitManager;
    public UnitData unitTemplate;

    [Header("UI Elements")]
    public Button saveButton;
    public Button loadButton;
    public Button resetButton;
    public Button randomizeButton;

    public TMP_InputField presetNameInput;

    public Slider outlineSizeSlider;
    public Image outlineColorPreview;
    public FlexibleColorPicker fcp_Outline;
    public Image skinColorPreview;
    public FlexibleColorPicker fcp_Skin;
    public Image primaryColorPreview;
    public FlexibleColorPicker fcp_Primary;
    public Image secondaryColorPreview;
    public FlexibleColorPicker fcp_Secondary;

    [Header("Spawn Settings")]
    public Vector3 spawnPosition;
    private Unit spawnedUnit;

    private UnitCosmeticData currentData;

    private void Start()
    {
        if (unitManager == null)
            unitManager = FindObjectOfType<ManagerUnits>();

        SpawnUnit();

        // Setup UI callbacks
        saveButton.onClick.AddListener(SavePreset);
        loadButton.onClick.AddListener(LoadPreset);
        resetButton.onClick.AddListener(ResetToDefault);
        randomizeButton.onClick.AddListener(RandomizeColors);

        outlineSizeSlider.onValueChanged.AddListener(OnOutlineSizeChanged);

        // Initialize color previews
        UpdateColorPreviews();
    }

    private void SpawnUnit()
    {
        if (unitTemplate == null)
        {
            Debug.LogError("Missing UnitData template reference.");
            return;
        }

        spawnedUnit = unitManager.SpawnUnit(unitTemplate, spawnPosition);
        spawnedUnit.NickName = "CustomizeUnit";

        currentData = spawnedUnit.GetCurrentCosmeticData();
    }

    #region Color Editing

    public void OnSelectSkinColor()
    {
       // Color newColor = GetRandomBrightColor();
        Color newColor = fcp_Skin.color;
        currentData.colorSkin = newColor;
        skinColorPreview.color = newColor;
        ApplyCurrentData();
    }

    public void OnSelectPrimaryColor()
    {
        //Color newColor = GetRandomBrightColor();
        Color newColor = fcp_Primary.color;
        currentData.colorMain = newColor;
        primaryColorPreview.color = newColor;
        ApplyCurrentData();
    }

    public void OnSelectSecondaryColor()
    {
        //Color newColor = GetRandomBrightColor();
        Color newColor = fcp_Secondary.color;
        currentData.colorSecondary = newColor;
        secondaryColorPreview.color = newColor;
        ApplyCurrentData();
    }

    public void OnSelectOutlineColor()
    {
        //Color newColor = GetRandomBrightColor();
        Color newColor = fcp_Outline.color;
        currentData.outlineColor = newColor;
        outlineColorPreview.color = newColor;
        ApplyCurrentData();
    }

    private void OnOutlineSizeChanged(float value)
    {
        currentData.outlineSize = value;
        ApplyCurrentData();
    }

    private void ApplyCurrentData()
    {
        if (spawnedUnit != null)
            spawnedUnit.ApplyCosmeticData(currentData);
    }

    private void UpdateColorPreviews()
    {
        skinColorPreview.color = currentData.colorSkin;
        primaryColorPreview.color = currentData.colorMain;
        secondaryColorPreview.color = currentData.colorSecondary;
        outlineColorPreview.color = currentData.outlineColor;
        outlineSizeSlider.value = currentData.outlineSize;
    }

    private Color GetRandomBrightColor()
    {
        return Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.7f, 1f);
    }

    #endregion

    #region Preset Operations

    private void SavePreset()
    {
        string presetName = presetNameInput.text.Trim();
        if (string.IsNullOrEmpty(presetName))
        {
            Debug.LogWarning("Enter a preset name first.");
            return;
        }

        UnitCosmeticSaveSystem.SaveCosmetics(spawnedUnit, presetName);
    }

    private void LoadPreset()
    {
        string presetName = presetNameInput.text.Trim();
        if (string.IsNullOrEmpty(presetName))
        {
            Debug.LogWarning("Enter a preset name first.");
            return;
        }

        UnitCosmeticSaveSystem.LoadCosmetics(spawnedUnit, presetName);
        currentData = spawnedUnit.GetCurrentCosmeticData();
        UpdateColorPreviews();
    }

    private void ResetToDefault()
    {
        spawnedUnit.ApplyCosmeticData(unitTemplate.defaultCosmetics);
        currentData = spawnedUnit.GetCurrentCosmeticData();
        UpdateColorPreviews();
    }

    private void RandomizeColors()
    {
        currentData.colorSkin = GetRandomBrightColor();
        currentData.colorMain = GetRandomBrightColor();
        currentData.colorSecondary = GetRandomBrightColor();
        currentData.outlineColor = GetRandomBrightColor();
        currentData.outlineSize = Random.Range(0.001f, 0.02f);
        ApplyCurrentData();
        UpdateColorPreviews();
    }

    #endregion
}
