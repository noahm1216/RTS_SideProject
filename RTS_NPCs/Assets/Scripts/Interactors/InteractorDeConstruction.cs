using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// <para>
/// Interactor defines objects like trees or buildings || De-Construction defines behavior of building or destroying that object
/// </para>
/// </summary>
public class InteractorDeConstruction : MonoBehaviour //  LAST LEFT OFF: i was consolidating Building_Behavior and Collectible_Behavior into 1 script  and simplifying the HP changes
{
    public int health { get; private set; } = 100;
    public float aboveGroundY = 5;
    public GameObject rootVisObj, inProgressObj;
    public TextMeshPro debugTextHP;

    private bool enableAsComplete;
    private Vector3 storedLocalScale = Vector3.zero; // || Z = height

    private SimpleInteractor ref_SimpleInteractor;
    private SphereCollider ref_Collider;
    // health bars
    // dmg vfx

    
       // TODO: Consolidate non vital and vital... just try to collect our references once, then run the check if we're missing any || to do later because its technically not causing issues now and Im feeling gooooood (on a roll)
       //       maybe we have a "bool SafeCheckPassed()" which runs through all our essentials and can be called first at ANY function in this script
    private bool NoVitalMissingReferences()
    {
        string missingRefText = "";
        //if (!Manager_Objects.instance) missingRefText += $"\nMissing: Manager_Objects.instance: {transform.name}"; // this throws an error even if in the scene
        if (!rootVisObj) missingRefText += $"\nMissing: rootVisObj: {transform.name}";
        if (!inProgressObj) missingRefText += $"\nMissing: inProgressObj: {transform.name}";

        if (string.IsNullOrEmpty(missingRefText))
            return true;
        else
        { print("WARNING: Missing Content" + missingRefText); return false; }
    }

    private void MissingNonVitalReferences(string _msg)
    {
        print($"WARNING: Missing Non-Vital-Reference on {transform.name}\n" + _msg);
    }

    private void GrabReferences()
    {
        if (!ref_Collider)
            TryGetComponent(out ref_Collider);
        if (!ref_SimpleInteractor)
            TryGetComponent(out ref_SimpleInteractor);
    }

    private bool IsAResource()
    {
        if (ref_SimpleInteractor)
        {
            if (ref_SimpleInteractor.listedExperience == UnitExperienceHistory.ExperienceType.Collectable)
                return true;
            else
                return false;
        }
        else
            MissingNonVitalReferences("ref_SimpleInteractor || Called by OnEnable()");

        return false;
    }

    public bool InteractorStillAvailableToDeConstruct()
    {
        if (IsAResource()) // if a resource (like a tree) it is NOT DeConstructable if there is ZERO hp || it is STILL DeConstrusctable if there is more than 0 hp
            return health > 0;
        else // if it's a building (like a house) it's NOT DeConstructable if it's at max hp || it is STILL DeConstructable if there is less than 100 hp
            return health < 100;
    }

    public int BalanceHP(int _amount)
    {
        //print($"BalanceHP: Was - {health} ... Summed with - {_amount} ... to get {health + _amount}");
        int leftover = 0;
        health += _amount;

        if (health > 100)
        { leftover = health-100; health = 100; HPReactionFull(); }
        if(health < 0)
        { leftover = health * -1; health = 0; HPReactionZero(); }

        return leftover;
    }

    private void HPReactionFull()
    {
        print($"{transform.name} has full HP");
        SetCollider(IsAResource());
    }

    private void HPReactionZero()
    {
        print($"{transform.name} has zero HP");
        SetCollider(!IsAResource());
    }

    private void CheckInProgressObj()
    {
        inProgressObj.SetActive(health < 100);      
    }

    private void SetHeight()
    {
        float newVerticalHeight = 0;

        if (IsAResource())
        {
            if (health < 100)
                newVerticalHeight = (aboveGroundY * health / 100) - aboveGroundY;  // math is not what I want
        }
        else
        {
            if (health < 100)
                newVerticalHeight = (aboveGroundY * (health / 100)) - aboveGroundY;
        }
        rootVisObj.transform.position = new Vector3(rootVisObj.transform.position.x, newVerticalHeight, rootVisObj.transform.position.z);
    }

    private void SetSize()
    {
        if (storedLocalScale== Vector3.zero)
            storedLocalScale = rootVisObj.transform.localScale;

        Vector3 newSize = storedLocalScale;

        if (IsAResource())
        {
            if (health < 100) // want resources to feel skinnier as they get smaller || Z = height
            {
                newSize.x = storedLocalScale.x * health / 100;
                newSize.y = storedLocalScale.y * health / 100;
                //newSize.z = health / 100;
            }
        }
        else
        {
            if (health < 100) // want buildings to feel grand as they get taller
            {
                //newSize.x = health / 100;
                //newSize.y =  health / 100;
                newSize.z = storedLocalScale.z * health / 100;
            }
        }
        rootVisObj.transform.localScale = newSize;
    }

    private void ResetHealth()
    {
        if (IsAResource())
        {
            if (enableAsComplete) health = 0;
            else health = 100;
        }
        else
        {
            if (enableAsComplete) health = 100;
            else health = 0;
        }
    }

    private void SetCollider(bool _enabled)
    {
        if (ref_Collider)
            ref_Collider.enabled = _enabled;
    }

    private void ConnectToManagerObjects(bool _addObjs)
    {
        if (_addObjs)
        {
            //print($"Adding Building: {transform.name}");
            if (IsAResource())
                Manager_Objects.collectableObjects.Add(gameObject);
            else
                Manager_Objects.buildingObjects.Add(gameObject);
        }
        else
        {
            if (IsAResource())
                Manager_Objects.collectableObjects.Remove(gameObject);
            else
                Manager_Objects.buildingObjects.Remove(gameObject);
        }
    }

    private void UpdateHPDisplay()
    {
        if (debugTextHP)
            debugTextHP.text = $"HP: {health}";
    }

    public void ResetStats()
    {
        ResetHealth();        
        SetHeight();
        SetSize();
        CheckInProgressObj();
        UpdateHPDisplay();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (!NoVitalMissingReferences())
            return;               
       
        // get refs
        GrabReferences();
        if (ref_SimpleInteractor)
            enableAsComplete = ref_SimpleInteractor.thisInteractorFinished;        
        ResetStats();
        SetCollider(IsAResource() && InteractorStillAvailableToDeConstruct() || !IsAResource() && !InteractorStillAvailableToDeConstruct());
        ConnectToManagerObjects(true);

        // TODO: Right now we start trees at full, but we could create a system where they grow on their own over time and the health you get is whatever their HP is at the time (same for buildings decay over time)
        //          IF we do this, it should be through the OBJ manager which could run through every tree every X seconds and call a "TimePassing" function which then handles either a "growth" or "decay" function depending on the tree's situation
    }// end of OnEnable()

    private void OnDisable()
    {
        if (!NoVitalMissingReferences())
            return;

        ResetStats();
        ConnectToManagerObjects(false);

    }// end of OnDisable()
       

    public int ChangeResource(int _amount)
    {
        if (!NoVitalMissingReferences())
            return 0;       

        if (InteractorStillAvailableToDeConstruct()) // we are not allowed to use this interactor anymore because it's done (either built or collected)
        {           
            print($"Resource-{transform.name} is being changed by {_amount}");
            int leftover = BalanceHP(_amount); // TODO: this should not spit out 100
            if (leftover != 0) _amount -= Mathf.Abs(leftover);
            SetHeight();
            SetSize();
            CheckInProgressObj();
        }
        else // if the resource/building is still able to be interacted 
        {
            print($"this interactor ({transform.name})  has nothing to take or give ");
        }
        SetCollider(IsAResource() && InteractorStillAvailableToDeConstruct() || !IsAResource() && !InteractorStillAvailableToDeConstruct());
        UpdateHPDisplay();
        return Mathf.Abs(_amount);
    }// end of TakeResource()

    
}
