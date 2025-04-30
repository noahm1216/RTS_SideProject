using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> Interactor Handler for simple events </para>
/// </summary>
public class SimpleInteractor : MonoBehaviour
{

    public string actionName;
    public UnitExperienceHistory.ExperienceType listedExperience;

    //if it's a building
    public enum BuildingType { None, Resource, House, Library, Trader }
    public BuildingType myBuilding;

    // if it's a resource
    public enum ResourceType { None, Wood, Stone, Gold, Food }
    public ResourceType myResource;

    public bool thisInteractorFinished = false;
    public UnityEvent OnInteraction;

    private Building_Behavior ref_Building_Behavior;
    private Collectable_Behavior ref_Collectable_Behavior;

    private void OnEnable()
    {
        if (!ref_Building_Behavior)
            TryGetComponent(out ref_Building_Behavior);
        if (!ref_Collectable_Behavior)
            TryGetComponent(out ref_Collectable_Behavior);


        if (string.IsNullOrEmpty(actionName))
        {
            if (myBuilding == BuildingType.Resource)
                actionName = myResource.ToString();
            else
                actionName = myBuilding.ToString();
        }
    }


    public UnitExperienceHistory InteractNow(NPC_Behavior _interactor)
    {
        print($"Interacted by: {_interactor.name} - Able To interact: {AbleToInteract()}");

        thisInteractorFinished = !AbleToInteract();

        if (thisInteractorFinished)
        {
            //print($"This SimpleInteractor: {transform.name} is looking for...\nA Tag to tell it's done. A Tag on {_interactor.name}");           
            return null;
        }
        else
        {
            OnInteraction?.Invoke(); // TODO: think of a way to communicate if a resource is available, or a building is done being built
            return new UnitExperienceHistory(actionName, listedExperience, 1);
        }

    }

    public bool AbleToInteract()
    {
        if (ref_Building_Behavior && ref_Building_Behavior.BuildingFullHealth())
            return false;
        if (ref_Collectable_Behavior && ref_Collectable_Behavior.ResourceStillAvailable() == false)
            return false;

        return true;
    }
  

   

}// end of SimpleInteractor class
