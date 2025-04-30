using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// <para> The main universal components of any npc</para>
/// </summary>
public class NPC_Behavior : MonoBehaviour
{
    // SEARCH "TODO" to find action item notes left previously

    //any major references 
    // - UI will be a child script 
    // - Navigation will be a child script
    // - animations will be a child script

    public bool showDebug;

    //isSelected
    public bool isSelected;
    // isSelected Visual References
    public GameObject activeVisObj;
    public SkinnedMeshRenderer playerOutlineMesh;
    private string shaderOutlineProperty = "_FadeExterior";
    private Vector2 shaderOutlineRange = new Vector2(1, 0.5f); // 1 = none, 0.5 = outline


    //-------------------------Information about this unit

    //healtPoints
    public int healtPoints = 1;
    // health points range
    private Vector2 healthPointsRange = new Vector2(0, 100);
    //speedMove
    public float speedMove = 1.0f;
    // speed move range
    public Vector2 speedMoveRange = new Vector2(0, 1);
    //actionRange
    //actionCooldown
    private float actionCooldownTime = 5.5f;
    //actionCooldownTimer
    private float actionCooldownTimer;
    //carryAmount
    [Tooltip("the amount this unit can carry. a static number that experience will multiply")]
    public Vector3Int carryAmountStarting = new Vector3Int(0,3,3); // X = current || Y = Normal Max || Z = Multiplied Max (based on XP)
    // the actual items we are carrying (can take this from the SigTar)
    public SimpleInteractor.ResourceType collectedResource;



    //the list of unique experiences we have (for ex: all trees count as one experience)
    public List<UnitExperienceHistory> myExperience = new List<UnitExperienceHistory>();


    //-------------------Signal Target information 

    [HideInInspector]
    public Transform mySigTar;
    private Vector3 lastSigTarPos;
    private bool commandedFromSignal;
    [HideInInspector]
    public Vector3 mySigTarPosition;
    [HideInInspector]
    public bool reachedSigTar;
    // for sending signal to interactable
    private SimpleInteractor mySigTarInteractor;

    // variables for animation
    [HideInInspector]
    public bool isMoving, takingAction;

    // Start is called before the first frame update
    void OnEnable()
    {
        SendCommand.targetClick += SignalTarget;
        Manager_Objects.npcObjects.Add(gameObject);
        SelectThisUnit(false);
    }// end of OnEnable()

    void OnDisable()
    {
        SendCommand.targetClick -= SignalTarget;
        Manager_Objects.npcObjects.Remove(gameObject);
    }// end of OnDisable()

    public void SignalTarget(RaycastHit _sigTar)
    {   // if this unit is actively selected during the signal
        if (isSelected) 
        {
            if(mySigTar)
                lastSigTarPos = mySigTar.position;
            mySigTarInteractor = null;
            mySigTar = _sigTar.transform;
            mySigTarPosition = _sigTar.point;
            reachedSigTar = false;
            commandedFromSignal = true;
        }
    }//end of SignalTarget(Transform _sigTar)

    // SendTarget is like SignalTarget, except that one takes from Static Event Clicks, and this one is referenced through any other sharing / functions
    public void SendTarget(Transform _senTar)
    {
        if(showDebug)
            print($"Unit: {transform.name} - is going to: {_senTar.name}");
        if (mySigTar)
            lastSigTarPos = mySigTar.position;
        mySigTarInteractor = null;
        mySigTar = _senTar;
        mySigTarPosition = _senTar.position;
        reachedSigTar = false;
        commandedFromSignal = false;
    }// end of SendTarget(Transform _senTar)


    public void SelectThisUnit(bool select)
    {
        isSelected = select;
        // visuals
        if(activeVisObj)
            activeVisObj.SetActive(select);
        if (playerOutlineMesh && !select)
            playerOutlineMesh.material.SetFloat(shaderOutlineProperty, shaderOutlineRange.x);
        if (playerOutlineMesh && select)
            playerOutlineMesh.material.SetFloat(shaderOutlineProperty, shaderOutlineRange.y);        

    }// end of SelectThisUnit()

    public void Update()
    {// anytime we press the mouse it should remove our selection
        if (Input.GetMouseButtonDown(0)) // TODO - change this to account for UI menus in the future
            SelectThisUnit(false); //               for example only when we click on a non-ui object

        if (reachedSigTar) // if we are within range to our target
            CalculateActionCooldown();

        if (isMoving)
        {
            ResetActionTimer();
            takingAction = false;
        }
    }// end of Update()

    public void OnMouseUp()
    {
        // TODO --- This command should send from a left-click raycast or from trigger collide with drag select
        SelectThisUnit(true); 
    }// end of OnMouseUp()


    private void CalculateActionCooldown()
    {
        //print("I have reached the target");

        if (mySigTar && mySigTar.tag == "Collectable" || mySigTar && mySigTar.tag == "Building") // mySigTar.tag == "NPC" //------------------------ TODO: Add condition for if we click on an NPC
        {
            print($"interacting with sigTar: {mySigTar.tag}");
            // get a reference || --> AS LONG AS NOT A UNIT
            if (!mySigTarInteractor)
            { mySigTarInteractor = mySigTar.transform.GetComponent<SimpleInteractor>(); print($"Grabbed reference to mySigtar {mySigTar.tag} - SimpleInteractor"); }

            // if there is work to be done then begin
            if (!mySigTarInteractor.thisInteractorFinished)
                takingAction = true; // this could work for REPAIR / BUILDING || or RESOURCE gathering
            else
            {
                //if(showDebug)
                print("calc cooldown: this is where we can drop off resources or repair a building");
                // is a building
                if (mySigTarInteractor.listedExperience == UnitExperienceHistory.ExperienceType.Building)
                {
                    //if (showDebug)
                    print("calc cooldown: IS A BUILDING");
                    // if resource house
                    if (mySigTarInteractor.myBuilding == SimpleInteractor.BuildingType.Resource) // TODO: change this to a switch case of the different types
                    {
                        TradeResources(null, !commandedFromSignal); // MAYBE WE ADD RESOURCE TO A STATIC MANAGER // ------------------- SOME ERROR HERE NULL REF - NOT SURE WHY YET
                        if (showDebug)
                            print("calc cooldown: should have just dropped off our resource");
                    }// end of is resource house

                }// end of is building
            }

            if (takingAction && Time.time > actionCooldownTimer + actionCooldownTime)
            {
                if (showDebug)
                    print("Finished A Round of Action");
                if(mySigTar.tag == "Collectable" && carryAmountStarting.x >= carryAmountStarting.z) // TODO: add new variables for our carry amount based on our resource experience
                {
                    if (showDebug)
                        print("UNIT CAPACITY FULL");
                    RemoveTarget();
                    FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false);
                }

                ResetActionTimer();               
                CheckExperience(mySigTarInteractor.InteractNow(this)); // TODO: this is where we are sending and grabbing the experience data, but we need to check if the resource / building is legit first 
                // TODO: we want a way to communicate our experience and how much of the SimpleInteractor is providing us. Maybe we can setup a return path that involves the SimpleInteractor bridging the gap between Unit and Object
            }
            
        }
        if (!mySigTar) // if we don't have a target to refer to anymore
        {
            print($"NO sigTar found: ");
            //store the last position we were at
            lastSigTarPos = transform.position;
            //if we have resources
            if (collectedResource != SimpleInteractor.ResourceType.None && carryAmountStarting.x > 0)
            {
                if (showDebug)
                    print("need to call the function to drop off any resources we might have");
                reachedSigTar = false;
                takingAction = false;
                FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false);
            }
        }
            

    }// end of CalculateActionCooldown()

    private void CheckExperience(UnitExperienceHistory _unitExperienceHistory)
    {
        if (showDebug)
            print("verifying experience");

        if (_unitExperienceHistory == null) // we completed whatever we were doing
        {
            print("we were NOT able to interact anymore");
            RemoveTarget();
            CalculateActionCooldown();
        }
        else // if we have something to work on
        {

            // if we have any experience
            if (myExperience.Count > 0)
            {
                bool foundExpMatch = false;
                // check our EXP
                for (int e = 0; e < myExperience.Count; e++)
                {
                    if (myExperience[e].actionName == _unitExperienceHistory.actionName)
                    {
                        myExperience[e].numberOfActionsCompleted += _unitExperienceHistory.numberOfActionsCompleted;
                        foundExpMatch = true;
                        break;
                    }
                }
                //if no match then we need to add the experience
                if (!foundExpMatch)
                    myExperience.Add(_unitExperienceHistory);
            }
            // if no experience yet
            else
                myExperience.Add(_unitExperienceHistory);

            // depending on which unit we selected we can take an action
            switch (_unitExperienceHistory.listedExperience)
            {
                case UnitExperienceHistory.ExperienceType.Unit:
                    // TODO: add function that heals or damages unit based on context
                    break;
                case UnitExperienceHistory.ExperienceType.Collectable:
                    CollectResourceBasedOnExperience(_unitExperienceHistory);
                    break;
                case UnitExperienceHistory.ExperienceType.Building:
                    // TODO: add function that when building / repair is finished we look for another building that needs the same
                    break;
                default:
                    if (showDebug)
                        print($"ERR: CHK EXP - on {transform.name} - recieved unrecognized EXP: {_unitExperienceHistory.listedExperience}");
                    break;
            }
        }
    }// end of CheckExperience()


    public void ResetActionTimer()
    {
        actionCooldownTimer = Time.time;
    }// end of ResetTimer()


    public void RemoveTarget()
    {
        reachedSigTar = false;
        reachedSigTar = false;
        takingAction = false;
        mySigTarInteractor = null;
    }// end of RemoveTarget()


    
    public void CollectResourceBasedOnExperience(UnitExperienceHistory _unitExperienceHistory)
    {
        SimpleInteractor.ResourceType previousResource = collectedResource; // TODO: add a check that if we are targeting a new resource we set our carry amount to 0
        collectedResource = mySigTarInteractor.myResource;

        // if we have a resource type we can add our experience to it
        if(collectedResource != SimpleInteractor.ResourceType.None)
        {
            // if our current carry amount is greather than our max
            if(carryAmountStarting.x >= carryAmountStarting.z) // TODO: add new variables for our carry amount based on our resource experience
            {
                if (showDebug)
                    print("UNIT DONE CARRYING");
                RemoveTarget();
                FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false);
            }
            else
            {
                if (showDebug)
                    print($"UNIT ADDING RESOURCE: {collectedResource}");
                carryAmountStarting.x++;
            }
        }

    }// end of CollectResourceBasedOnExperience


    public void FindClosestBuilding(Vector3 _position, SimpleInteractor.BuildingType _buildingType, bool _onlyUnfinishedOnes)
    {
        if (showDebug)
            print("finding closest resource building");

        List<Transform> allTargetedBuildings = new List<Transform>();
        if (Manager_Objects.buildingObjects.Count == 0)
            return;

        // make an update list of only those buildings
        foreach (GameObject buildingObj in Manager_Objects.buildingObjects)
        {
            SimpleInteractor buildingInteractorScript = buildingObj.GetComponent<SimpleInteractor>();
            // if we are currently building then we want to find other buildings to build
            if (_onlyUnfinishedOnes && !buildingInteractorScript.thisInteractorFinished)
                allTargetedBuildings.Add(buildingObj.transform);
            // else we are just looking for already built versions of this building
            else if (buildingInteractorScript.myBuilding == _buildingType && buildingInteractorScript.thisInteractorFinished)
                allTargetedBuildings.Add(buildingObj.transform);
        }

        Transform closestBuilding = null;
        float thatBuildingsDistance = 0;
        // find the closest
        foreach(Transform buildingTran in allTargetedBuildings)
        {
            float dist = Vector3.Distance(buildingTran.position, transform.position);
            // if it's the first building we check or the closest thus far, then set it
            if(thatBuildingsDistance == 0 || dist < thatBuildingsDistance)
            {
                closestBuilding = buildingTran;
                thatBuildingsDistance = dist;
            }
        }
        if (showDebug)
            print($"CLOSEST RESOURCE BUILDING: {closestBuilding.name} || Dist: {thatBuildingsDistance}");
        // if we made it this far we should have a building to go to
        if(closestBuilding)
            SendTarget(closestBuilding);
    }// end of FindClosestResourceBuilding


    public void FindClosestResource(Vector3 _position, SimpleInteractor.ResourceType _resourceType) //---------TODO: add condition that checks for a reasonable distance
    {
        if (showDebug)
            print($"finding closest resource for last position of: {_resourceType}");

        List<Transform> allResourcesOfSameType = new List<Transform>();
        if (Manager_Objects.collectableObjects.Count == 0)
            return;

        // make an update list of only those buildings
        foreach (GameObject collectableObj in Manager_Objects.collectableObjects)
        {
            //print($"Resource: {collectableObj.transform.name}");
            SimpleInteractor resourceInteractorScript = collectableObj.GetComponent<SimpleInteractor>();
            // if we found a matching resource that is still up for grabs then add it
            if (resourceInteractorScript.myResource == _resourceType && !resourceInteractorScript.thisInteractorFinished)
                allResourcesOfSameType.Add(collectableObj.transform);
        }

        Transform closestResource = null;
        float thatResourceDistance = 0;
        // find the closest
        foreach (Transform resourceTran in allResourcesOfSameType)
        {
            float dist = Vector3.Distance(resourceTran.position, _position);
            // if it's the first building we check or the closest thus far, then set it
            if (thatResourceDistance == 0 || dist < thatResourceDistance)
            {
                closestResource = resourceTran;
                thatResourceDistance = dist;
            }
        }
        if (showDebug)
            print($"CLOSEST RESOURCE BUILDING: {closestResource.name} || Dist: {thatResourceDistance}");
        // if we made it this far we should have a building to go to
        if (closestResource)
            SendTarget(closestResource);

    }// end of FindClosestResourceBuilding


    private void TradeResources(Transform _target, bool returnToLastTarget)
    {
        // add carry amount to taker // MAYBE WE ADD RESOURCE TO A STATIC MANAGER
        // clear our carry amount
        if (Manager_Objects.CanChangeResources(collectedResource, carryAmountStarting.x))
            carryAmountStarting.x = 0;

        if (returnToLastTarget) // this lets a worker continue working on their task
            FindClosestResource(lastSigTarPos, collectedResource);

    }// end of TradeResources()

}// end of NPC_Behavior class



[System.Serializable]
public class UnitExperienceHistory
{
    public string actionName;
    public enum ExperienceType {Unit, Collectable, Building }
    public ExperienceType listedExperience;
    public int numberOfActionsCompleted;

    public UnitExperienceHistory(string _actName, ExperienceType _listExp, int _numOfActCom)
    {
        actionName = _actName;
        listedExperience = _listExp;
        numberOfActionsCompleted = _numOfActCom;        
    }
}// end of UnitExperienceHistory class

