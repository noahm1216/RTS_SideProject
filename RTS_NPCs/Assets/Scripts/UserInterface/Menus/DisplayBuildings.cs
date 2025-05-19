using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayBuildings : MonoBehaviour
{
    public Transform buildingUIDataObjTemplate;
    public SO_Buildings[] allPossibleBuildings;

    public void OnEnable()
    {
        if (buildingUIDataObjTemplate)
            buildingUIDataObjTemplate.gameObject.SetActive(false);

        PopulateBuildingIcons();
    }

    public void PopulateBuildingIcons()
    {
        if (!buildingUIDataObjTemplate || allPossibleBuildings.Length == 0)
        { Debug.Log("WARNING: Missing Reference to template"); return; }

        for (int buildings = 0; buildings < allPossibleBuildings.Length; buildings++)
        {
            if (allPossibleBuildings[buildings].isUnlocked == false)
                continue;

            bool buildingRepresented = false;
            BuildingUIData buildUIData = null;

            //print($"Checking Building: {allPossibleBuildings[buildings].buildingName}");

            for (int uiChild = 0; uiChild < buildingUIDataObjTemplate.parent.childCount; uiChild++)
            {
                if (buildingUIDataObjTemplate.parent.GetChild(uiChild) == buildingUIDataObjTemplate || uiChild <= buildings)
                    continue;

                buildingUIDataObjTemplate.parent.GetChild(uiChild).TryGetComponent(out buildUIData);
                if (buildUIData)
                {
                    buildUIData.InjectData(this, allPossibleBuildings[buildings]);
                    buildingRepresented = true;
                    break;
                }
            }

            if (!buildingRepresented)
            {
                Transform cloneUI = Instantiate(buildingUIDataObjTemplate, buildingUIDataObjTemplate.parent);
                cloneUI.TryGetComponent(out buildUIData);
                if (buildUIData)
                    buildUIData.InjectData(this, allPossibleBuildings[buildings]);

                cloneUI.gameObject.SetActive(true);
            }

            buildingRepresented = false;
            buildUIData = null;

            //print($"Checking Building: {allPossibleBuildings[buildings].buildingName}");
        }
    }

    public void SpawnBuilding(SO_Buildings _buildingToSpawn)
    {
        if (!_buildingToSpawn)
        { Debug.Log("WARNING: Missing Reference to buildingToSpawn"); return; }

        Manager_Spawner.ChangeBuildingSelected(_buildingToSpawn);
        //print("SpawningBuilding: " + _buildingToSpawn.name);
    }
}
