using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingUIData : MonoBehaviour
{
    public Image buildingImage;
    public TextMeshProUGUI nameText, resourceText_Wood, resourceText_Food, resourceText_Stone, resourceText_Gold;
    private DisplayBuildings ref_DisplayBuildings;
    private SO_Buildings ref_BuildingData;

    public void InjectData(DisplayBuildings _refDB, SO_Buildings ref_SOB)
    {
        ref_DisplayBuildings = _refDB;
        ref_BuildingData = ref_SOB;
        UpdateUIInformation();
    }

    public void UpdateUIInformation()
    {
        if (!ref_DisplayBuildings || !ref_BuildingData)
        { Debug.Log("WARNING: Missing Reference to scripts"); return; } // missing references

        if (buildingImage)
            buildingImage.sprite = ref_BuildingData.buildingSprite;
        if (nameText)
            nameText.text = ref_BuildingData.buildingName;
        if (resourceText_Wood)
            resourceText_Wood.text = ref_BuildingData.resourceCosts.resourceWood.ToString();
        if (resourceText_Food)
            resourceText_Food.text = ref_BuildingData.resourceCosts.resourceFood.ToString();
        if (resourceText_Stone)
            resourceText_Stone.text = ref_BuildingData.resourceCosts.resourceStone.ToString();
        if (resourceText_Gold)
            resourceText_Gold.text = ref_BuildingData.resourceCosts.resourceGold.ToString();


        transform.name = "CloneUI_" + ref_BuildingData.buildingName;
    }

    public void OnClick()
    {
        print("Clicked - " + nameText);
        if (ref_DisplayBuildings)
            ref_DisplayBuildings.SpawnBuilding(ref_BuildingData);
    }

}
