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

    //if it's a building
    public enum LastAction { None, wasBuilding, wasGathering, wasDelivering, wasWalking } // wasFighting


    //healtPoints
    public int healtPoints = 10;
    // health points range
    private Vector2 healthPointsRange = new Vector2(0, 100);
    //speedMove
    public float speedMove = 1.0f;
    // speed move range
    public Vector2 speedMoveRange = new Vector2(0, 1);
    //automaticSearchingRange // TODO: Add a radius variable that all units consider when looking automatically for things
    //actionCooldown
    public float actionCooldownTime = 3.5f; // NOTE: this was 3.5 during testing || TODO: add a comparable that we can multiply based on our experience to reduce our collecting time as we get better
    //actionCooldownTimer
    private float actionCooldownTimer;
    //carryAmount
    [Tooltip("the amount this unit can carry. a static number that experience will multiply")]
    public Vector3Int carryAmountStarting = new Vector3Int(0, 3, 3); // X = current || Y = Normal Max || Z = Multiplied Max (based on XP)
    //references to the last items, objects, or units we were interacting with
    private UnitExperienceHistory.ExperienceType mostRecentInteractionExp;
    private SimpleInteractor.BuildingType mostRecentBuildingExp;
    private SimpleInteractor.ResourceType mostRecentResourceExp;
    public LastAction mostRecentAction;



    //the list of unique experiences we have (for ex: all trees count as one experience skill/item)
    public List<UnitExperienceHistory> myExperience = new List<UnitExperienceHistory>();


    //-------------------Signal Target information 
    [Range(1.8f, 5f)]
    public float tarDistTolerance = 2; // sweet spot is somewhere arnound 1.8-3;
    public bool randomizeDistTolerance = true;

    public Transform mySigTar { get; private set; }
    public Transform lastSigTar { get; private set; }
    public Vector3 mySigTarPosition { get; private set; }
    public Vector3 lastSigTarPosition { get; private set; } // NOTE: MIGHT NOT need this
    private bool commandedFromSignal;
    private SimpleInteractor ref_SigTar_SimpleInteractor; // for sending signal to interactable

    // variables for animation
    public bool isMoving { get; private set; }
    public bool takingAction { get; private set; }

    // Start is called before the first frame update
    void OnEnable()
    {
        //SendCommand.targetClick += SignalTarget;
        Manager_Objects.npcObjects.Add(gameObject);
        SelectThisUnit(false);
    }// end of OnEnable()

    void OnDisable()
    {
        //SendCommand.targetClick -= SignalTarget;
        Manager_Objects.npcObjects.Remove(gameObject);
        SelectThisUnit(false);
    }// end of OnDisable()

    public void SelectThisUnit(bool select)
    {
        isSelected = select;
        // visuals
        if (activeVisObj)
            activeVisObj.SetActive(select);
        if (playerOutlineMesh && !select)
            playerOutlineMesh.material.SetFloat(shaderOutlineProperty, shaderOutlineRange.x);
        if (playerOutlineMesh && select)
            playerOutlineMesh.material.SetFloat(shaderOutlineProperty, shaderOutlineRange.y);

    }// end of SelectThisUnit()

    public void OnMouseUp()
    {
        // TODO --- This command should send from a left-click raycast or from trigger collide with drag select
        SelectThisUnit(true);
    }// end of OnMouseUp()

    public void SignalTarget(RaycastHit _sigTar)
    {   // if this unit is actively selected during the signal
        if (isSelected)
        {
            if (mySigTar) // store reference to return
            { lastSigTar = mySigTar; lastSigTarPosition = lastSigTar.position; }
            ref_SigTar_SimpleInteractor = null;
            mySigTar = _sigTar.transform;
            mySigTarPosition = _sigTar.point;
            if (mySigTar)
                mySigTar.TryGetComponent(out ref_SigTar_SimpleInteractor);
            else
                ResetActionTimer(true, false);
            //print($"COMPARING POSITIONS: object position - {mySigTar.position} VS hitpoint - {_sigTar.point} ");
            ResetActionTimer(true, true);
            commandedFromSignal = true;
        }
    }//end of SignalTarget(Transform _sigTar)

    // SendTarget is like SignalTarget, except that one takes from Static Event Clicks, and this one is referenced through any other sharing / functions
    public void SendTarget(Transform _senTar)
    {
        if (showDebug) print($"Unit: {transform.name} - is going to: {_senTar.name}");

        if (_senTar == null)
            mySigTarPosition = Vector3.zero;
        else if (lastSigTar && _senTar == lastSigTar)
        {
            lastSigTarPosition = lastSigTarPosition;
        }
        else
        {
            if (mySigTar)
                lastSigTarPosition = transform.position;
        }

        if (mySigTar) // store reference to return
            lastSigTar = mySigTar;
        ref_SigTar_SimpleInteractor = null;
        mySigTar = _senTar;
        if (mySigTar)
            mySigTar.TryGetComponent(out ref_SigTar_SimpleInteractor);

        if (_senTar == null)
            mySigTarPosition = Vector3.zero;
        else
            mySigTarPosition = _senTar.position;
        commandedFromSignal = false;

    }// end of SendTarget(Transform _senTar)

    public bool ReachedTarget()
    {
        if (mySigTar == null)
        { if (showDebug) Debug.Log($"WARNING: Unit {transform.name} - tried to reach a target with no target selected."); return false; }

        float dist = Vector3.Distance(transform.position, mySigTarPosition);
        //print($"Dist To Sigtar = {Math.Round(dist*100) / 100.0f}");

        return dist <= tarDistTolerance * 1.5f;
    }

    public void ChangeIsMoving(bool _isMoving)
    {
        if (showDebug && isMoving != _isMoving)
            print($"Changing Moving from {isMoving} - to - {_isMoving}");
        isMoving = _isMoving;
    }

    public bool UnitCarryCapacityFull()
    {
        if (carryAmountStarting.x >= carryAmountStarting.z)
            carryAmountStarting.x = carryAmountStarting.z;
        if (carryAmountStarting.x <= 0)
            carryAmountStarting.x = 0;

        return carryAmountStarting.x >= carryAmountStarting.z;
    }

    public void ResetCarryCapacity()
    {
        carryAmountStarting.x = 0;
    }

    public void Update()
    {// anytime we press the mouse it should remove our selection
        if (Input.GetMouseButtonDown(0)) // TODO - change this to account for UI menus in the future
            SelectThisUnit(false); //               for example only when we click on a non-ui object      

        if (mySigTar && ReachedTarget() && !isMoving) // if we are within range to our target & not moving
            DecideAction();//CalculateActionCooldown();

        if (isMoving && mostRecentAction != LastAction.wasWalking) // make sure we're not improperly animating / taking actions
            ResetActionTimer(true, false);

        if (!ReachedTarget())
            mostRecentAction = LastAction.wasWalking;
    }// end of Update()  

    private bool ActionCooldownIsReady()
    {
        if (Time.time > actionCooldownTimer + actionCooldownTime)
        {
            if (showDebug) print("ActionTimeReady");
            takingAction = true;
            ResetActionTimer(false, true);
            return true;
        }
        return false;
    }

    public void ResetActionTimer(bool _resetTakeAction, bool _resetTimer)
    {
        if (showDebug) print("ResettingActionTimer");
        if (_resetTimer)
            actionCooldownTimer = Time.time;
        if (_resetTakeAction)
            takingAction = false;
    }

    private void DecideAction()
    {
        if (mySigTar && !ref_SigTar_SimpleInteractor)
            mySigTar.TryGetComponent(out ref_SigTar_SimpleInteractor);

        if (!ref_SigTar_SimpleInteractor)
        { Debug.Log($"WARNING: Unit cannot find SimpleInteractor for this object... STOPPING CalculateActionCooldown() on: {transform.name}"); return; }

        if (ActionCooldownIsReady()) // if we are not ready to begin acting then we should stop here
        {
            // figure out what we are interacting with
            switch (ref_SigTar_SimpleInteractor.listedExperience)
            {
                case UnitExperienceHistory.ExperienceType.Building:
                    //if (showDebug) print("Action - Building");
                    if (showDebug) print("Action - Building");
                    ActionBuilding();
                    break;
                case UnitExperienceHistory.ExperienceType.Collectable:
                    if (showDebug) print("Action - Collecting");                    
                    ActionCollectable();
                    break;
                case UnitExperienceHistory.ExperienceType.Unit:
                    if (showDebug) print("Action - Unit");
                    ActionUnit();
                    break;
                default:
                    Debug.Log($"WARNING: Unit SigTar SimpleInteractor experience type ({ref_SigTar_SimpleInteractor.listedExperience}) not considered... STOPPING CalculateActionCooldown() on: {transform.name}");
                    break;
            }
        }
    }

    private void ActionBuilding()
    {
        if (showDebug) print("AB(0) - Action Building called");
        if (!ref_SigTar_SimpleInteractor)
        { if (showDebug) print($"AB(0.1) - No Sigtar.SimpleInteractor avaialble on: {mySigTar.transform.name} --> during ActionBuilding()"); return; }

        if (ref_SigTar_SimpleInteractor.thisInteractorFinished)  // if building is completed
        {
            if (showDebug) print("AB(1.0) - the building we want to interact with is complete");
            // if we were just building
            if (mostRecentAction == LastAction.wasBuilding)
            {
                if (showDebug) print("AB(1.1) - Our last action was building || we should FindClosestBuilding"); // TODO: Based on the building  we were building... we can assume the unit's task might be to collect resources for it (build wood farm... then go cut trees)
                FindClosestBuilding(transform.position, mostRecentBuildingExp, true); //== false) { Debug.Log($"WOW - NO MORE: {"unbuilt"} buildings to drop off at"); SendTarget(null); } // here is where we would add code to change tasks if FindClosestBuilding() == false
            }
            else
            {
                if (showDebug) print("AB(1.2) - we were not just building");
                // if it's a resource 
                if (ref_SigTar_SimpleInteractor.myBuilding == SimpleInteractor.BuildingType.Resource)
                {
                    if (showDebug) print("AB(1.3) - this built building is a reosurce building");
                    // AND if it accepts our resource type 
                    if (ref_SigTar_SimpleInteractor.myResource == SimpleInteractor.ResourceType.None || ref_SigTar_SimpleInteractor.myResource == mostRecentResourceExp)
                    {
                        if (showDebug) print("AB(1.4) - we can deposit at this one");
                        if (carryAmountStarting.x > 0) { print("AB(1.4.1) - we are depositing our resource"); TradeAResources(mostRecentResourceExp, carryAmountStarting.x); }// AND if we have resources --> then drop them off and return to our target
                        if (!commandedFromSignal) // if we dropped off a resource because we were at full capacity then we should return back to it
                        { if (showDebug) print($"AB(1.5) - We should return to our last target {lastSigTar.name} || ActionBuilding()"); SendTarget(lastSigTar); }
                        mostRecentAction = LastAction.wasDelivering;
                    }
                }
                // else we may want to do other things but right now Im not sure of them (like traders or libraries or etc)
            }
        }
        else // this building is NOT finished (or needs repairs)
        {
            if (showDebug) print("AB(2.0) - The building still needs repair");
            // assign our building type
            mostRecentBuildingExp = ref_SigTar_SimpleInteractor.myBuilding;
            // store that we were building (to clarify for when we finish building)
            mostRecentAction = LastAction.wasBuilding;
            // begin building it up
            UnitExperienceHistory newExpToCompare = ref_SigTar_SimpleInteractor.InteractNow(this, 1);
            // check if our interactor is complete and if so find the next closest one (maybe within a range?)
            if (newExpToCompare == null || ref_SigTar_SimpleInteractor.thisInteractorFinished)
            {
                if (showDebug) print("AB(2.1) - Finished this building and need to find another");
                FindClosestBuilding(transform.position, mostRecentBuildingExp, true); //== false) {  Debug.Log($"WOW - NO MORE: {"unbuilt"} buildings to drop off at"); SendTarget(null);  }
            }
            else mostRecentInteractionExp = UnitExperienceHistory.ExperienceType.Building;
            CheckExperience(newExpToCompare);
        }
    }

    private void ActionCollectable()
    {
        if (showDebug) print("AC(0-START) - Called ActionCollectable()");

        // check if this is the same type as the last collectable we collected
        if (ref_SigTar_SimpleInteractor.myResource != mostRecentResourceExp)
        {
            if (showDebug) print("AC(1) - Resource Mismatch ... dropping our current supplies");
            ResetCarryCapacity(); //  then reset our variables          
            mostRecentResourceExp = ref_SigTar_SimpleInteractor.myResource; // assign our resource type                    
        }

        if (ref_SigTar_SimpleInteractor.thisInteractorFinished && !UnitCarryCapacityFull()) // check if the resource is depleted 
        { if (showDebug) print("AC(2) - Resource is depleted but we have room for more"); if (FindClosestResource(mySigTarPosition, mostRecentResourceExp) == false) { print("AC(2.1) - No more resources nearby"); SendTarget(null); }; } // if it is then find a new one nearby it

        if (showDebug) print($"AC(3) - Checking Backpack: {carryAmountStarting.x}/{carryAmountStarting.z} ... CapacityFul = {UnitCarryCapacityFull()}");

        if (UnitCarryCapacityFull()) // check if we are full
        {
            if (showDebug) print("AC(3.1) - Unit's carrying capacity is FULL. trying to find a dropoff spot");
            FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false);
            //{ Debug.Log($"AC(3.2) - WOW - NO MORE: {mostRecentResourceExp} buildings to drop off at"); SendTarget(null); } // if we are then let's find the closest resource building       
        }
        else // still got room to collect
        {
            if (showDebug) print("AC(4) - Still got room to collect");
            if (ref_SigTar_SimpleInteractor)
            {
                print("AC(4.1) - Lets collect");
                UnitExperienceHistory newExpToCompare = ref_SigTar_SimpleInteractor.InteractNow(this, -1); // begin building it up
                if (newExpToCompare == null) FindClosestResource(transform.position, mostRecentResourceExp);  //print("AC(4.1.1) - WOW no close resources in range"); }
                else mostRecentInteractionExp = UnitExperienceHistory.ExperienceType.Collectable;
                mostRecentAction = LastAction.wasGathering;
                CheckExperience(newExpToCompare);
            }
            else
                print("AC(4.2) - Cant collect");
        }

        if (showDebug) print("AC(0-END) - Called ActionCollectable()");
    }

    private void ActionUnit() // TODO: take a moment to DESIGN out what I want to happen here
    {
        print($"NOTE: No Action Information for ActionUnit() - on: {transform.name}");
        // NOTE: check if friendly or not ... check range of weapon 
        //       if friendly, could transfer resources, or heal, or follow... 
        //       if not friendly maybe attack or something else
        // this hasnt been figured out yet 

        mostRecentInteractionExp = UnitExperienceHistory.ExperienceType.Unit;
        //mostRecentAction = LastAction.wasFighting;
    }



    private void CheckExperience(UnitExperienceHistory _newExpToCompare) // This function adds or ammends our experience list
    {
        if (showDebug) print("verifying experience");


        if (_newExpToCompare != null) // if we have something to work on
        {
            if (myExperience.Count > 0)  // if we have any experience
            {
                bool foundExpMatch = false;
                for (int e = 0; e < myExperience.Count; e++) // check our current EXP
                {
                    if (myExperience[e].actionName == _newExpToCompare.actionName)
                    {
                        myExperience[e].numberOfActionsCompleted += _newExpToCompare.numberOfActionsCompleted;
                        myExperience[e].totalInteractorValuesReturned += _newExpToCompare.totalInteractorValuesReturned;
                        if (showDebug) print($"Experience makes this action's carry max to be: {carryAmountStarting.y} * {myExperience[e].numberOfActionsCompleted} / {10} (+ 3) = {3 + (carryAmountStarting.y * (myExperience[e].numberOfActionsCompleted / 10))}");
                        carryAmountStarting.z = 3 + (carryAmountStarting.y * (myExperience[e].numberOfActionsCompleted / 10)); // give 3 for every 10 experience doing something
                        foundExpMatch = true;
                        break;
                    }
                }
                if (!foundExpMatch) //if no match then we need to add the experience
                {
                    myExperience.Add(_newExpToCompare);
                    carryAmountStarting.z = carryAmountStarting.y;
                }
            }
            else // if no experience yet
                myExperience.Add(_newExpToCompare);

            ChangeResourceBasedOnExperience(_newExpToCompare);
        }
        else // if we got a null exp to compare (which shouldnt happen) then we need to find something new to do
        {
            ResetActionTimer(true, false);
            DecideAction();
        }
    }// end of CheckExperience()

    public void ChangeResourceBasedOnExperience(UnitExperienceHistory _newExpToCompare)
    {
        //SimpleInteractor.ResourceType previousResource = mostRecentResourceExp; // TODO: add a check that if we are targeting a new resource we set our carry amount to 0

        if (showDebug) print($"UNIT ADDING RESOURCE: {mostRecentResourceExp}");


        switch (mostRecentInteractionExp)
        {
            case UnitExperienceHistory.ExperienceType.Collectable:
                if (showDebug) print($"Checking If I Can Carry More: {carryAmountStarting.x} / {carryAmountStarting.z} = {UnitCarryCapacityFull()} || ChangeResourceBasedOnExperience(_newExpToCompare)");
                if (mostRecentResourceExp == SimpleInteractor.ResourceType.None) Debug.Log($"WARNING: Resource type is set to none for target: {mySigTar.name}"); // check for null resource
                carryAmountStarting.x += (_newExpToCompare.totalInteractorValuesReturned); // NOTE: if we want to change how much Resource WE get, this is the final section its applied || could do something like: "carryAmount.x = valReturned + (exp/10)"
                //if (UnitCarryCapacityFull()) { if(FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false) == false) Debug.Log($"WOW - NO MORE: {mostRecentResourceExp} buildings to drop off at"); SendTarget(null); }
                if (UnitCarryCapacityFull()) { FindClosestBuilding(transform.position, SimpleInteractor.BuildingType.Resource, false); }
                break;
            case UnitExperienceHistory.ExperienceType.Building: // TODO: when we are taking resource, we will need a more complex system if we have more than just WOOD... like how can we tell if a building needs WOOD || or STONE to repair / build ? 
                TradeAResources(SimpleInteractor.ResourceType.Wood, _newExpToCompare.totalInteractorValuesReturned * -1); // TODO: We will want some modifications made based on our building experience
                break;
            case UnitExperienceHistory.ExperienceType.Unit:
                break;
            default:
                break;
        }
    }// end of ChangeResourceBasedOnExperience

    private void TradeAResources(SimpleInteractor.ResourceType _resType, int _changeBy)
    {
        if (Manager_Objects.CanChangeResources(mostRecentResourceExp, _changeBy))
        {
            if (_changeBy > 0) // if "_changeBy" is positive we are delivering a resource || otherwise we are taking the resource
                carryAmountStarting.x = 0;
        }
        else
            Debug.Log($"WARNING: Unit: ({transform.name}) - Could not DeliverResources({mostRecentResourceExp},{carryAmountStarting.x})");
    }



    public bool FindClosestBuilding(Vector3 _position, SimpleInteractor.BuildingType _buildingType, bool _onlyUnfinishedOnes) // TODO: Add a radius variable that all units consider when looking automatically for things
    {
        if (showDebug) print("finding closest resource building");

        // create a list we can populate with buildings that match our filters passed
        // based on those filters try to find the closes one that fits the requirements

        List<Transform> allTargetedBuildings = new List<Transform>();
        if (Manager_Objects.buildingObjects.Count == 0)
        { if (showDebug) print("FCB(1) - DIDNT Find any buildings in our manager"); return false; } // NOTE: I think somewhere here or below we are getting STUCK and not finding new buildings when we just REPAIRED

        // make an update list of only those buildings
        for (int i = 0; i < Manager_Objects.buildingObjects.Count; i++)
        {
            SimpleInteractor buildingInteractorScript = null;
            Manager_Objects.buildingObjects[i].TryGetComponent(out buildingInteractorScript);
            if (!buildingInteractorScript) { Debug.Log($"WARNING: Unable to grabe SimpleInteractor from: {Manager_Objects.buildingObjects[i].transform.name}"); continue; }

            if (showDebug) print($"Finding Closest Building: ...\nTarget: OnlyUnfinished-{_onlyUnfinishedOnes} || Type-{_buildingType}\nComparing Against: {Manager_Objects.buildingObjects[i].transform.name} || unFinished = {!buildingInteractorScript.thisInteractorFinished} || Type = {buildingInteractorScript.myBuilding}");

            //checks for consideration
            if (allTargetedBuildings.Contains(Manager_Objects.buildingObjects[i].transform)) { print("FCB(2) - this building was already added to the new list"); continue; }
            // if we were trying to automate building, then we want to find other buildings nearby to help finish building
            if (_onlyUnfinishedOnes && !buildingInteractorScript.thisInteractorFinished) { print($"FCB(3) - we wanted unfinished buildings and this was one! {Manager_Objects.buildingObjects[i].transform.name}"); allTargetedBuildings.Add(Manager_Objects.buildingObjects[i].transform); continue; }
            // if we are just looking for already built versions of this building
            if (buildingInteractorScript.myBuilding == _buildingType && buildingInteractorScript.thisInteractorFinished) { allTargetedBuildings.Add(Manager_Objects.buildingObjects[i].transform); continue; }

            if (showDebug) print($"FCB(4): Building: {Manager_Objects.buildingObjects[i].transform.name} ... was not a match");
        }

        // a final check to make sure if we have a building we're on now, isnt in consideration
        if (mostRecentAction == LastAction.wasBuilding && allTargetedBuildings.Contains(mySigTar)) // EXCLUDING the building we just have... unless it isnt in the list when we check
            allTargetedBuildings.Remove(mySigTar);

        Transform closestBuilding = null;
        float thatBuildingsDistance = 0;
        // find the closest
        for (int i = 0; i < allTargetedBuildings.Count; i++)
        {
            if (showDebug) print("FCB(5) - checking for the closest");
            float dist = Vector3.Distance(allTargetedBuildings[i].position, transform.position);
            // if it's the first building we check or the closest thus far, then set the distance as the new closest
            if (thatBuildingsDistance == 0 || dist < thatBuildingsDistance)
            {
                closestBuilding = allTargetedBuildings[i];
                thatBuildingsDistance = dist;
            }
        }
        if (showDebug) print($"CLOSEST RESOURCE BUILDING: {closestBuilding.name} || Dist: {thatBuildingsDistance}");
        // if we made it this far we should have a building to go to
        if (closestBuilding)
        { SendTarget(closestBuilding); return true; }

        return false;
    }// end of FindClosestResourceBuilding

    public bool FindClosestResource(Vector3 _position, SimpleInteractor.ResourceType _resourceType) // TODO: Add a radius variable that all units consider when looking automatically for things
    {
        if (showDebug) print($"finding closest resource for last position of: {_resourceType}");

        List<Transform> allResourcesOfSameType = new List<Transform>();
        if (Manager_Objects.collectableObjects.Count == 0)
            return false;

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
        if (showDebug) print($"CLOSEST RESOURCE BUILDING: {closestResource.name} || Dist: {thatResourceDistance}");
        // if we made it this far we should have a building to go to
        if (closestResource)
        { SendTarget(closestResource); return true; }

        return false;
    }// end of FindClosestResourceBuilding


}// end of NPC_Behavior class



[System.Serializable]
public class UnitExperienceHistory
{
    public string actionName;
    public enum ExperienceType {Unit, Collectable, Building }
    public ExperienceType listedExperience;
    public int numberOfActionsCompleted;
    public int totalInteractorValuesReturned;

    public UnitExperienceHistory(string _actName, ExperienceType _listExp, int _numOfActCom, int _totalIntrVals)
    {
        actionName = _actName;
        listedExperience = _listExp;
        numberOfActionsCompleted = _numOfActCom;
        totalInteractorValuesReturned = _totalIntrVals;
    }
}// end of UnitExperienceHistory class

