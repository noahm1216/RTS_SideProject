using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class UnitData
{
    // EXPECTED STATS || (a higher level manager can handle holding onto perks and things we accrue)
    // constitution ( hp )
    // dexterity (speed moving / haf of speedBuilding)
    // strength (dmg / half of speedBuilding)
    // intelligence (half of upgradeXp / relationship with nature)
    // wisdom ( half of upgradeXp / experience learning skills)


    // EXPECTED DATA
    // bool isSelected
    // int hp
    // float speedMove
    // float speedAction
    // enum - ACTION TAKING {Idle, Walking, Gathering, Building, Attacking, Relaxing}
    // list<ActivitiesDone> (a custom list with actions and integers so each time an activity is done like 'chop wood' its added to the list)


    // EXPECTED COSMETICS
    // colorSkin
    // colorScheme1
    // colorScheme2
}

public class Cosmetic_Unit_Data : MonoBehaviour
{
    public bool canDragToScale;
    public ScaleOnDrag[] scaleOnDragComponents;
    public Transform[] bodyParts { get; private set; }
    public Vector3[] bodyScaleData { get; private set; }
    public Vector3[] factoryResetScale { get; private set; }


    void OnEnable()
    {
        print("CONTROLS:" +
            "\n_________" +
            "\nPress: 1 - to get all body parts" +
            "\nPress: 2 - to store current parts scale data" +
            "\nPress: 3 - to reset body parts to original model settings" +
            "\nPress: 4 - to reset body parts to stored scale data" +
            "\nPress: 5 - to toggle click and dragging to scale body parts");
        GetAllBodyParts();
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { print("GetBody Parts"); GetAllBodyParts(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { print("Store Parts"); StoreAllScaleData(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { print("Reset Stored Parts Factory"); ResetBodyToScaleData(true); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { print("Reset Stored Parts Previous Save"); ResetBodyToScaleData(false); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { print($"Changing Can Drag On Scale To: {!canDragToScale}"); ChangeCanDragToScale(!canDragToScale); }
    }

    public void ChangeCanDragToScale(bool _canDragToScale)
    {
        canDragToScale = _canDragToScale;
        if (scaleOnDragComponents.Length == 0){ Debug.LogWarning("Missing Script Refs"); return; }
        for (int i = 0; i < scaleOnDragComponents.Length; i++)
            scaleOnDragComponents[i].enabled = canDragToScale;
    }

    public void ValueChanged()
    {
        StoreAllScaleData();
    }

    public void GetAllBodyParts()
    {
        bodyParts = new Transform[0]; // reset array

        // Get all Transforms in the hierarchy, including the parent
        bodyParts = GetComponentsInChildren<Transform>();
        scaleOnDragComponents = new ScaleOnDrag[bodyParts.Length];
        factoryResetScale = new Vector3[bodyParts.Length];

        for (int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i].GetComponent<ScaleOnDrag>()) scaleOnDragComponents[i] = bodyParts[i].GetComponent<ScaleOnDrag>();
            factoryResetScale[i] = bodyParts[i].localScale;
        }
    }


    public void StoreAllScaleData()
    {
        if (bodyParts.Length == 0) { Debug.LogWarning("Missing Transform Refs"); return; }

            bodyScaleData = new Vector3[bodyParts.Length];
            for (int i = 0; i < bodyParts.Length; i++)
                bodyScaleData[i] = bodyParts[i].localScale;
    }

    public void ResetBodyToScaleData(bool _toFactory)
    {
        if (bodyParts.Length == 0) { Debug.LogWarning("Missing Transform Refs"); return; }

        Vector3[] replacerData = new Vector3[0];
        if (_toFactory) replacerData = factoryResetScale;
        else replacerData = bodyScaleData;
        if (replacerData == null) { Debug.LogWarning($"Couldnt Store Data Due To Missing References: '_toFactorY' = {_toFactory}"); return; }
        for (int i = 0; i < replacerData.Length; i++) // apply the local scale changes
            bodyParts[i].localScale = replacerData[i];
    }

    public void FillAllScaleData(Vector3[] _bodyScaleData) // send the data of stored character scale customizations
    {
        GetAllBodyParts();
        bodyScaleData = _bodyScaleData;
        ResetBodyToScaleData(false);
    }
}
