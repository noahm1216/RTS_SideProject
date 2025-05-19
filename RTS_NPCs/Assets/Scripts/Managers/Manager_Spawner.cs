using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Spawner : MonoBehaviour
{
    public static Manager_Spawner instance { get; private set; }
    public static Camera camScreen { get; private set; }
    public static Transform mousePositionObject { get; private set; }

    public LayerMask buildableLayers;

    public static Transform currentDisplayModel { get; private set; }
    public static SO_Buildings currentDisplayBuilding { get; private set; }


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

    }// end of Awake()

    private void Start()
    {
        if (!camScreen)
            camScreen = Camera.main;

        if (!mousePositionObject)
            mousePositionObject = new GameObject("MousePositionObject").transform;
    }

    private void FixedUpdate()
    {
        if (mousePositionObject && GetMousePosition3D() != Vector3.zero)
            mousePositionObject.position = GetMousePosition3D();
    }


    public Vector3 GetMousePosition3D()
    {
        Ray ray = camScreen.ScreenPointToRay(Input.mousePosition);
        Vector3 rayHitPos = Vector3.zero;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildableLayers))
            return hit.point;

        return rayHitPos;
    }

    public static void ChangeBuildingSelected(SO_Buildings _soBuilding) // show the display || or hide it if NULL is passed
    {
        currentDisplayBuilding = _soBuilding;
        if(currentDisplayBuilding != null)
        {
            if (currentDisplayModel)
                Destroy(currentDisplayModel.gameObject);
            currentDisplayModel = Instantiate(currentDisplayBuilding.prefabToPreview, mousePositionObject);
        }
        else
        {
            if (currentDisplayModel)
                Destroy(currentDisplayModel.gameObject);
            currentDisplayModel = null;
        }
    }
 
}




