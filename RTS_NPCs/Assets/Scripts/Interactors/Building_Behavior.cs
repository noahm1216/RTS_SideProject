using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building_Behavior : MonoBehaviour
{
    public int buildingHealth { get; private set; } = 0;

    public GameObject BuildingRoot, BuildingConstruction;
    public Vector2 belowGroundY_AboveGroundY;
    public bool buildingDoneBuilding;
    public bool needsRepair; // if we need repair (then we need to set the simple interactor as NOT DONE)

    private BoxCollider buildingCollider;
    private SimpleInteractor mySimpleInteractor;
    // health bars
    // dmg vfx


    // Start is called before the first frame update
    void OnEnable()
    {
        Manager_Objects.buildingObjects.Add(gameObject);

        if (!buildingDoneBuilding)
            BuildingRoot.transform.position = new Vector3(BuildingRoot.transform.position.x, belowGroundY_AboveGroundY.x, BuildingRoot.transform.position.z);
        else
            buildingHealth = 100;

        BuildingConstruction.SetActive(!buildingDoneBuilding);
        buildingCollider = transform.GetComponent<BoxCollider>();
        mySimpleInteractor = transform.GetComponent<SimpleInteractor>();
    }// end of OnEnable()

    private void OnDisable()
    {
        Manager_Objects.buildingObjects.Remove(gameObject);
    }// end of OnDisable()

    public void AddConstruction()
    {
        print("Adding Construction");

        if (buildingDoneBuilding) // this could be good for repairing a damage building
        {
            print($"this building is fully built: {transform.name}");
        }
        else // if the building is still being built first time
        {
            BuildingRoot.transform.position += new Vector3(0, 1, 0);
            buildingHealth += 10; // TODO: pass a variable involving our experience... OR perhaps we use the builder to call the function multiple times for every X amount of experience they have

            if (BuildingFullHealth())
            {
                buildingHealth = 100;
                BuildingRoot.transform.position = new Vector3(BuildingRoot.transform.position.x, belowGroundY_AboveGroundY.y, BuildingRoot.transform.position.z);
                buildingDoneBuilding = true;
                BuildingConstruction.SetActive(false);
            }    
                
        }
        if (BuildingFullHealth())
            mySimpleInteractor.thisInteractorFinished = buildingDoneBuilding;
    }

    public bool BuildingFullHealth()
    {
        return buildingHealth >= 100;
    }

}// end of Building_Behavior class
