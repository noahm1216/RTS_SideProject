using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> Handles the trigger enters and exits of the box selector </para>
/// </summary>
public class DragSelectCollider : MonoBehaviour
{
    public List<GameObject> allTriggeredObjects = new List<GameObject>();

    private string unitTag = "Unit";

    public void OnTriggerEnter(Collider trig)
    {
        if (trig.tag == unitTag)
        {
            allTriggeredObjects.Add(trig.gameObject);
            if (trig.GetComponent<NPC_Behavior>())
                trig.GetComponent<NPC_Behavior>().SelectThisUnit(true);
        }
    }// end of OnTriggerEnter

    public void OnTriggerExit(Collider trig)
    {
        if (trig.tag == unitTag)
        {
            allTriggeredObjects.Remove(trig.gameObject);
            if (trig.GetComponent<NPC_Behavior>())
                trig.GetComponent<NPC_Behavior>().SelectThisUnit(false);

        }
    }// end of OnTriggerExit



}// end of DragSelectCollider class
