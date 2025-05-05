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
    public BuildingType myBuilding; // NOTE: An error seems to keep happening where this variable is RESET on random Unity choices... that causes error

    // if it's a resource
    public enum ResourceType { None, Wood, Stone, Gold, Food }
    public ResourceType myResource;

    public bool thisInteractorFinished = false;
    public UnityEvent OnInteraction;

    private InteractorDeConstruction ref_InteractorDeConstruction;

    private void OnEnable()
    {
        if (listedExperience == UnitExperienceHistory.ExperienceType.Building && myBuilding == BuildingType.None)
            Debug.Log("ERROR: the ui has reset the tags of some buildings");

        if (!ref_InteractorDeConstruction)
            TryGetComponent(out ref_InteractorDeConstruction);       

        if (string.IsNullOrEmpty(actionName))
        {
            if (listedExperience == UnitExperienceHistory.ExperienceType.Collectable)
                actionName = listedExperience.ToString() + "_" + myResource.ToString();
            else
                actionName = listedExperience.ToString() + "_" + myBuilding.ToString() + "_" + myResource;
        }
    }


    public UnitExperienceHistory InteractNow(NPC_Behavior _interactor, int _valueChangeRequest)
    {
        //print($"Interacted by: {_interactor.name} - Able To interact: {AbleToInteract()}");

        thisInteractorFinished = !AbleToInteract(); 

        if (thisInteractorFinished)
        {
            print("ThisInteractorFinished = true ... Should it do anything else?"); // IF IT'S A BUILDING ... maybe
            //print($"This SimpleInteractor: {transform.name} is looking for...\nA Tag to tell it's done. A Tag on {_interactor.name}");           
            return null;
        }
        else
        {
            OnInteraction?.Invoke(); // TODO: think of a way to communicate if a resource is available, or a building is done being built                 
            if (ref_InteractorDeConstruction)
                return new UnitExperienceHistory(actionName, listedExperience, 1, ref_InteractorDeConstruction.ChangeResource(_valueChangeRequest));
            else
                return new UnitExperienceHistory(actionName, listedExperience, 1, _valueChangeRequest);
        }

    }

    public bool AbleToInteract()
    {
        if (ref_InteractorDeConstruction)
            return ref_InteractorDeConstruction.InteractorStillAvailableToDeConstruct();   

        return false;
    }
  

   

}// end of SimpleInteractor class
